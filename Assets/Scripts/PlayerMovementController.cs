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
    private int jumpsRemaining;

    [Header("Wall Slide & Jump")]
    [SerializeField] private float wallSlideSpeed = 1.5f;
    [SerializeField] private float wallJumpX = 8f;
    [SerializeField] private float wallJumpY = 14f;
    [SerializeField] private float wallJumpTime = 0.2f;
    [SerializeField] private LayerMask wallLayer;
    private bool isTouchingWall;
    private bool isWallSliding;
    private float wallJumpTimer;
    private int wallDirection;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    private bool isDashing;
    private float dashTimeLeft;
    private float dashCooldownTimer;
    private float facingDirection = 1f;

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
        if (isPaused) return;

        // ESC = pause
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }

        // DASH — reads Shift directly, no Input Actions needed
        if ((Keyboard.current.leftShiftKey.wasPressedThisFrame || 
             Keyboard.current.rightShiftKey.wasPressedThisFrame)
            && !isDashing && dashCooldownTimer <= 0)
        {
            StartDash();
        }

        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
        if (wallJumpTimer > 0) wallJumpTimer -= Time.deltaTime;
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
        if (horizontalInput != 0)
            skeletonAnimation.AnimationState.SetAnimation(0, ANIM_WALK, true);
        else
            skeletonAnimation.AnimationState.SetAnimation(0, ANIM_IDLE, true);
    }

    private void CheckWallSlide()
    {
        float rayLength = 0.7f;
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, rayLength, wallLayer);
        RaycastHit2D hitLeft  = Physics2D.Raycast(transform.position, Vector2.left,  rayLength, wallLayer);

        isTouchingWall = false;
        if (hitRight.collider != null && horizontalInput > 0) { isTouchingWall = true; wallDirection =  1; }
        else if (hitLeft.collider != null && horizontalInput < 0) { isTouchingWall = true; wallDirection = -1; }

        isWallSliding = isTouchingWall && !groundCheck.isGrounded && rb.linearVelocityY < 0;

        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, -wallSlideSpeed);
            jumpsRemaining = 1;
        }
    }

    private void FixedUpdate()
    {
        if (isPaused) return;

        CheckWallSlide();

        if (groundCheck.isGrounded && rb.linearVelocityY <= 0.1f)
            jumpsRemaining = maxJumps;

        // DASH movement
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

        // Normal movement
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

        // WALL JUMP
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(-wallDirection * wallJumpX, wallJumpY);
            wallJumpTimer = wallJumpTime;
            wasInAir = true; airTimeout = 0.1f;
            skeletonAnimation.AnimationState.SetAnimation(0, ANIM_JUMP, false);
            skeletonAnimation.timeScale = jumpAnimationSpeed;
            return;
        }

        // DOUBLE JUMP
        if (jumpsRemaining > 0)
        {
            float vel = jumpsRemaining < maxJumps ? jumpVelocity * 1.25f : jumpVelocity;
            jumpsRemaining--;
            rb.linearVelocity = new Vector2(rb.linearVelocityX, vel);
            skeletonAnimation.AnimationState.SetAnimation(0, ANIM_JUMP, false);
            skeletonAnimation.timeScale = jumpAnimationSpeed;
            wasInAir = true; airTimeout = 0.1f;
        }
    }

    public void OnDash(InputValue value)
    {
        if (isPaused) return;
        if (value.isPressed && !isDashing && dashCooldownTimer <= 0)
            StartDash();
    }
}