using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;
    private float moveInput;

    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float iceAcceleration = 10f;
    [SerializeField] private float iceSlowDown = 2.5f;
    private bool wasOnIce;

    [SerializeField] private float jumpForce = 6.0f;
    [SerializeField] private float jumpTime = 0.3f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private int remainingJumps;
    [SerializeField] private AudioClip jumpSound;
    private bool isJumping;
    private float jumpTimeCounter;
    private bool hasDoubleJump;

    [SerializeField] private GameObject lLeg;
    [SerializeField] private GameObject rLeg;

    [HideInInspector] public bool isFacingRight;

    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpHForce = 4f;
    [SerializeField] private float wallJumpVForce = 10f;
    [SerializeField] private float wallJumpControlLock = 0.17f;
    private bool isWallSliding;
    private bool canWallJump;
    private float wallJumpLockTimer;
    private bool hasWallAbilities;

    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private AudioClip dashSound;
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;
    private bool hasDash;

    [SerializeField] private float extraHeight = 0.15f;
    [SerializeField] private LayerMask tileSet;
    [SerializeField] private LayerMask icyTiles;

    private RaycastHit2D groundHit;
    private RaycastHit2D wallHit;

    [SerializeField] private float fallThreshold = -10f;
    private Vector3 spawnPos;
    private float defaultGravityScale;
    private bool isRespawning;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        spawnPos = transform.position;
        defaultGravityScale = rb.gravityScale;

        StartDirectionCheck();
    }

    private void Update()
    {
        if (isRespawning)
            return;

        if (transform.position.y < fallThreshold)
        {
            Respawn();
            return;
        }

        Move();
        Jump();
        Dash();
    }

    private void FixedUpdate()
    {
        if (moveInput < 0 || moveInput > 0)
        {
            TurnCheck();
        }
    }

    public void ConfigureForLevel(string levelID)
    {
        string[] parts = levelID.Split('-');

        if (parts.Length != 2 || !int.TryParse(parts[0], out int areaNumber) || areaNumber < 1 || areaNumber > 5)
            return;

        hasDoubleJump = areaNumber >= 2;
        hasDash = areaNumber >= 3;
        hasWallAbilities = areaNumber >= 4;

        maxJumps = hasDoubleJump ? 2 : 1;
        remainingJumps = maxJumps;

        canDash = true;
        isDashing = false;

        if (trailRenderer != null)
            trailRenderer.emitting = false;

    }

    private void Move()
    {
        if (isDashing)
            return;

        moveInput = UserInput.instance.moveInput.x;

        if (wallJumpLockTimer > 0f)
        {
            wallJumpLockTimer -= Time.deltaTime;
            WallSlide();
            animator.SetFloat("xVelocity", rb.velocity.x);
            return;
        }

        if (isOnIce())
        {
            wasOnIce = true;
        }
        else if (isGrounded())
        {
            wasOnIce = false;
        }

        if (isOnIce() || (!isGrounded() && wasOnIce))
        {
            float targetVelocityX = moveInput * moveSpeed;
            float acceleration = Mathf.Abs(moveInput) > 0.01f ? iceAcceleration : iceSlowDown;
            float newVelocityX = Mathf.MoveTowards(rb.velocity.x, targetVelocityX, acceleration * Time.deltaTime);

            rb.velocity = new Vector2(newVelocityX, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }

        WallSlide();

        animator.SetFloat("xVelocity", rb.velocity.x);
    }

    private void WallSlide()
    {
        if (!hasWallAbilities)
        {
            isWallSliding = false;
            animator.SetBool("isClinging", false);
            return;
        }

        if (isTouchingIcyWall())
        {
            isWallSliding = false;
            animator.SetBool("isClinging", false);
            return;
        }

        if (!isGrounded() && isTouchingWall() && rb.velocity.y <= 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
        }
        else
        {
            isWallSliding = false;
        }

        animator.SetBool("isClinging", isWallSliding);
    }

    private void Jump()
    {
        if (isDashing)
            return;

        if (isGrounded())
        {
            remainingJumps = maxJumps;
            canWallJump = true;
        }
        else if (hasWallAbilities && !isTouchingAnyWall())
        {
            canWallJump = true;
        }

        if (UserInput.instance.controls.Jumping.Jump.WasPerformedThisFrame())
        {
            if (hasWallAbilities && !isGrounded() && isTouchingWall())
            {
                if (canWallJump && !isTouchingIcyWall())
                {
                    WallJump();
                    SoundManager.instance.PlaySound(jumpSound);
                }

                return;
            }

            if (remainingJumps > 0)
            {
                isJumping = true;
                jumpTimeCounter = jumpTime;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                remainingJumps--;
                SoundManager.instance.PlaySound(jumpSound);
            }
        }

        if (UserInput.instance.controls.Jumping.Jump.IsPressed())
        {
            if (jumpTimeCounter > 0 && isJumping)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        if (UserInput.instance.controls.Jumping.Jump.WasReleasedThisFrame())
        {
            isJumping = false;
        }

        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isJumping", !isGrounded() && !isWallSliding);
    }

    private void WallJump()
    {
        bool touchingLeftWall = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.left, extraHeight, tileSet | icyTiles).collider != null;
        bool touchingRightWall = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.right, extraHeight, tileSet | icyTiles).collider != null;
        bool betweenWalls = touchingLeftWall && touchingRightWall;

        float horizontalForce = betweenWalls ? 0f : (isFacingRight ? -wallJumpHForce : wallJumpHForce);

        rb.velocity = new Vector2(horizontalForce, wallJumpVForce);

        canWallJump = false;
        remainingJumps = maxJumps;

        isJumping = false;
        jumpTimeCounter = 0f;
        wallJumpLockTimer = wallJumpControlLock;

        if (!betweenWalls)
            Turn();

        animator.SetBool("isClinging", false);
        isWallSliding = false;
    }

    private void StartDirectionCheck()
    {
        if (rLeg.transform.position.x > lLeg.transform.position.x)
        {
            isFacingRight = true;
        }
        else
        {
            isFacingRight = false;
        }
    }

    private void TurnCheck()
    {
        if (UserInput.instance.moveInput.x > 0 && !isFacingRight || UserInput.instance.moveInput.x < 0 && isFacingRight)
        {
            Turn();
        }
    }

    private void Turn()
    {
        if (isFacingRight)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;
        }
        else
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;
        }
    }

    private void Dash()
    {
        if (!hasDash || isDashing)
            return;

        if (UserInput.instance.controls.Dashing.Dash.WasPerformedThisFrame() && canDash)
        {
            StartCoroutine(Dashing());
            SoundManager.instance.PlaySound(dashSound);
        }
    }

    private IEnumerator Dashing()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        float dashDirection = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(dashDirection * dashingPower, 0f);
        trailRenderer.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        trailRenderer.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private bool isGrounded()
    {
        groundHit = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.down, extraHeight, tileSet | icyTiles);

        return groundHit.collider != null;
    }

    private bool isOnIce()
    {
        RaycastHit2D iceHit = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.down, extraHeight, icyTiles);
        return iceHit.collider != null;
    }

    private bool isTouchingWall()
    {
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        wallHit = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, direction, extraHeight, tileSet | icyTiles);

        return wallHit.collider != null;
    }

    private bool isTouchingIcyWall()
    {
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        RaycastHit2D icyWallHit = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, direction, extraHeight, icyTiles);

        return icyWallHit.collider != null;
    }

    private bool isTouchingAnyWall()
    {
        RaycastHit2D leftHit = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.left, extraHeight, tileSet | icyTiles);
        RaycastHit2D rightHit = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.right, extraHeight, tileSet | icyTiles);

        return leftHit.collider != null || rightHit.collider != null;
    }

    private void Respawn()
    {
        isRespawning = true;

        StopAllCoroutines();

        rb.velocity = Vector2.zero;
        rb.gravityScale = defaultGravityScale;
        rb.angularVelocity = 0f;
        transform.position = spawnPos;

        isDashing = false;
        canDash = true;
        isJumping = false;
        jumpTimeCounter = 0f;
        remainingJumps = maxJumps;
        canWallJump = true;
        isWallSliding = false;
        wasOnIce = false;
        wallJumpLockTimer = 0f;

        if (trailRenderer != null)
            trailRenderer.emitting = false;

        animator.SetBool("isJumping", false);
        animator.SetBool("isClinging", false);
        animator.SetFloat("xVelocity", 0f);
        animator.SetFloat("yVelocity", 0f);

        isRespawning = false;
    }
}
