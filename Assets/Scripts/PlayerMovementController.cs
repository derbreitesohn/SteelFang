using Spine.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float speed = 10f; 
    [SerializeField] private float jumpVelocity = 15f;
    [SerializeField] private GroundCheck groundCheck;

    [Header("Double Jump")]
    [SerializeField] private int maxJumps = 2;
    private int jumpsRemaining;

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
        {
            pauseMenuUI.SetActive(false);
        }
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
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

    private void FixedUpdate()
    {
        if (isPaused) return;
        if (groundCheck.isGrounded && rb.linearVelocityY <= 0.1f)
        {
            jumpsRemaining = maxJumps;
        }

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
        if (isPaused || isDashing) return;
        if (jumpsRemaining > 0 && value.isPressed)
        {
            float actualJumpVel = jumpVelocity;
            if (jumpsRemaining < maxJumps) 
            {
                actualJumpVel = jumpVelocity * 1.25f; 
            }

            jumpsRemaining--; 
            rb.linearVelocity = new Vector2(rb.linearVelocityX, actualJumpVel);
            skeletonAnimation.AnimationState.SetAnimation(0, ANIM_JUMP, false);
            skeletonAnimation.timeScale = jumpAnimationSpeed;
            wasInAir = true;
            airTimeout = 0.1f;
        }
    }

    public void OnDash(InputValue value)
    {
        if (isPaused) return;
        if (value.isPressed && !isDashing && dashCooldownTimer <= 0)
        {
            isDashing = true;
            dashTimeLeft = dashDuration;
            dashCooldownTimer = dashCooldown;
            rb.gravityScale = 0f;
        }
    }
}