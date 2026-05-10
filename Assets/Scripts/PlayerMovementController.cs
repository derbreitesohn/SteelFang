using Spine.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpVelocity = 15f;
    [SerializeField] private GroundCheck groundCheck;

    [Header("Double Jump")]
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float doubleJumpCooldown = 0.3f;
    private float doubleJumpTimer;
    private int jumpsRemaining;

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpX = 6f;
    [SerializeField] private float wallJumpY = 12f;
    [SerializeField] private float wallJumpTime = 0.15f;
    [SerializeField] private LayerMask wallLayer;
    private bool isTouchingWall;
    private float wallJumpTimer;
    private int wallDirection;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.8f;
    private bool isDashing;
    private float dashTimeLeft;
    private float dashCooldownTimer;
    private float facingDirection = 1f;

    [Header("Fall Gravity")]
    [SerializeField] private float fallGravityMultiplier = 2.5f; // makes fall feel snappier

    [Header("Animation")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private float defaultAnimationSpeed = 1f;
    [SerializeField] private float jumpAnimationSpeed = 2f;

    [Header("Pause")]
    [SerializeField] private GameObject pauseMenuUI;

    private Rigidbody2D rb;
    private float horizontalInput;
    private float originalXScale;
    private bool wasInAir;
    private float airTimeout;
    private bool isPaused = false;
    private float originalGravity;

    private const string ANIM_IDLE = "dance";
    private const string ANIM_WALK = "walk cycle";
    private const string ANIM_JUMP = "jump";

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalXScale = transform.localScale.x;
        skeletonAnimation.timeScale = defaultAnimationSpeed;
        originalGravity = rb.gravityScale;
    }

    void Start()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        // ESC must be checked BEFORE the isPaused return — otherwise you can never unpause!
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
            return;
        }

        if (isPaused) return;

        // Dash — only when grounded
        if ((Keyboard.current.leftShiftKey.wasPressedThisFrame ||
             Keyboard.current.rightShiftKey.wasPressedThisFrame)
            && !isDashing && dashCooldownTimer <= 0 && groundCheck.isGrounded)
        {
            StartDash();
        }

        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
        if (wallJumpTimer > 0) wallJumpTimer -= Time.deltaTime;
        if (doubleJumpTimer > 0) doubleJumpTimer -= Time.deltaTime;
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        dashCooldownTimer = dashCooldown;
        rb.gravityScale = 0f;
    }

    public void PauseGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void SetGroundAnimation()
    {
        if (isDashing) return;
        string targetAnim = horizontalInput != 0 ? ANIM_WALK : ANIM_IDLE;
        var currentTrack = skeletonAnimation.AnimationState.GetCurrent(0);
        if (currentTrack == null || currentTrack.Animation.Name != targetAnim)
            skeletonAnimation.AnimationState.SetAnimation(0, targetAnim, true);
    }

    private void CheckWallSlide()
    {
        float rayLength = 0.6f;
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, rayLength, wallLayer);
        RaycastHit2D hitLeft  = Physics2D.Raycast(transform.position, Vector2.left,  rayLength, wallLayer);

        isTouchingWall = false;
        if (hitRight.collider != null && horizontalInput > 0) { isTouchingWall = true; wallDirection =  1; }
        else if (hitLeft.collider != null && horizontalInput < 0) { isTouchingWall = true; wallDirection = -1; }
    }

    private void FixedUpdate()
    {
        if (isPaused) return;

        CheckWallSlide();

        // Reset jumps when grounded
        if (groundCheck.isGrounded && rb.linearVelocityY <= 0.1f)
            jumpsRemaining = maxJumps;

        // Snappier falling — apply extra gravity when going down
        if (!isDashing && rb.linearVelocityY < 0)
            rb.gravityScale = originalGravity * fallGravityMultiplier;
        else if (!isDashing)
            rb.gravityScale = originalGravity;

        // Dash movement
        if (isDashing)
        {
            dashTimeLeft -= Time.fixedDeltaTime;
            if (dashTimeLeft <= 0)
            {
                isDashing = false;
                rb.gravityScale = originalGravity;
                rb.linearVelocityX = 0f;
                if (groundCheck.isGrounded) SetGroundAnimation();
            }
            else
            {
                rb.linearVelocity = new Vector2(facingDirection * dashSpeed, 0f);
                return;
            }
        }

        // Normal horizontal movement
        if (wallJumpTimer <= 0)
            rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocityY);

        if (wasInAir) airTimeout -= Time.fixedDeltaTime;

        if (groundCheck.isGrounded && wasInAir && airTimeout <= 0)
        {
            wasInAir = false;
            skeletonAnimation.timeScale = defaultAnimationSpeed;
            SetGroundAnimation();
        }
    }

    public void OnMoveHorizontal(InputValue value)
    {
        if (isPaused) return;
        horizontalInput = value.Get<float>();
        if (horizontalInput != 0)
        {
            facingDirection = Mathf.Sign(horizontalInput);
            transform.localScale = new Vector3(
                horizontalInput > 0 ? originalXScale : -originalXScale,
                transform.localScale.y,
                transform.localScale.z
            );
            if (groundCheck.isGrounded) SetGroundAnimation();
        }
        else
        {
            if (!wasInAir) SetGroundAnimation();
        }
    }

    public void OnJump(InputValue value)
    {
        if (isPaused || isDashing || !value.isPressed) return;

        // Wall jump
        if (isTouchingWall && !groundCheck.isGrounded)
        {
            rb.linearVelocity = new Vector2(-wallDirection * wallJumpX, wallJumpY);
            wallJumpTimer = wallJumpTime;
            wasInAir = true;
            airTimeout = 0.1f;
            skeletonAnimation.AnimationState.SetAnimation(0, ANIM_JUMP, false);
            skeletonAnimation.timeScale = jumpAnimationSpeed;
            return;
        }

        // Normal / double jump
        if (jumpsRemaining > 0)
        {
            bool isFirstJump = jumpsRemaining == maxJumps;
            if (!isFirstJump && doubleJumpTimer > 0) return;

            jumpsRemaining--;
            if (!isFirstJump) doubleJumpTimer = doubleJumpCooldown;

            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpVelocity);
            skeletonAnimation.AnimationState.SetAnimation(0, ANIM_JUMP, false);
            skeletonAnimation.timeScale = jumpAnimationSpeed;
            wasInAir = true;
            airTimeout = 0.1f;
        }
    }

    public void OnDash(InputValue value)
    {
        if (isPaused) return;
        if (value.isPressed && !isDashing && dashCooldownTimer <= 0 && groundCheck.isGrounded)
            StartDash();
    }
}