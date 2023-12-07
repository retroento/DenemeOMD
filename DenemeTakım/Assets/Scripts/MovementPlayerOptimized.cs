using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class MovementPlayer : MonoBehaviour
{
    public float moveSpeed;
    public float jumpForceMin;
    public float jumpForceMax;
    public float maxJumpTime;
    public float dashDistance;
    public float dashDuration;
    public float dodgeDistance;
    public float dodgeDuration;
    public Tilemap groundTilemap;
    public Animator animator;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isShifting;
    private bool isDashing;
    private bool isDodging;
    private bool canDash = true;
    private bool canDodge = true;
    private bool isFacingRight = true;
    private bool isJumping;
    private float jumpTime;
    private bool isBulletTime;
    private float originalTimeScale;
    private bool hasJumped;
    private int doubleJumpCount;
    public int maxDoubleJumps = 1;
    public float doubleJumpForce = 5f;
    public float dashSpeed = 10f;
    

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalTimeScale = Time.timeScale;
    }

   
    void Update()
    {
        HandleMovementInput();
        CheckGrounded();
        CheckJumpInput();
        CheckDashInput();
        CheckDodgeInput();
    }

    void HandleMovementInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        Vector2 movement = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));

        if (isDashing)
        {
            movement = rb.velocity.normalized * dashSpeed;
        }

        rb.velocity = movement;

        FlipCharacter(horizontalInput);
    }

    void FlipCharacter(float horizontalInput)
    {
        if ((horizontalInput < 0 && isFacingRight) || (horizontalInput > 0 && !isFacingRight))
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    void CheckGrounded()
    {
        Vector3Int cellPosition = groundTilemap.WorldToCell(transform.position - new Vector3(0f, 0.5f, 0f));
        isGrounded = groundTilemap.HasTile(cellPosition);

        if (!isGrounded)
        {
            animator.SetBool("isFalling", true);
        }

        if (isGrounded)
        {
            hasJumped = false;
            doubleJumpCount = 0;
            animator.SetBool("isFalling", false);
        }
    }

    void CheckJumpInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                isJumping = true;
                jumpTime = 0f;
                rb.velocity = new Vector2(rb.velocity.x, jumpForceMin);
                animator.SetBool("isJumping", true);
            }
            else if (!hasJumped && doubleJumpCount < maxDoubleJumps)
            {
                hasJumped = true;
                doubleJumpCount++;
                rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
                animator.SetBool("isJumping", true);
            }
        }

        if (Input.GetButton("Jump") && isJumping && jumpTime < maxJumpTime)
        {
            jumpTime += Time.deltaTime;
            float jumpForce = Mathf.Lerp(jumpForceMin, jumpForceMax, jumpTime / maxJumpTime);
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
            jumpTime = 0f;
            animator.SetBool("isJumping", false);
        }
    }

    void CheckDashInput()
    {
        if (Input.GetButtonDown("Dash") && canDash)
        {
            if (!isShifting)
            {
                isShifting = true;
                ToggleBulletTime();
            }
            else
            {
                isShifting = false;
                ToggleBulletTime();
                Vector2 dashDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

                if (dashDirection != Vector2.zero)
                {
                    StartCoroutine(Dash(dashDirection));
                }
            }
        }
    }

    void ToggleBulletTime()
    {
        if (!isBulletTime)
        {
            isBulletTime = true;
            Time.timeScale = 0.2f;
        }
        else
        {
            isBulletTime = false;
            Time.timeScale = originalTimeScale;
        }
    }

    void CheckDodgeInput()
    {
        if (Input.GetButtonDown("Dodge") && canDodge)
        {
            StartCoroutine(Dodge());
        }
    }

    IEnumerator Dash(Vector2 dashDirection)
    {
        animator.SetBool("isDashing", true);
        isDashing = true;
        canDash = false;

        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("enemyLayer");
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        rb.velocity = dashDirection * dashDistance / dashDuration;

        yield return new WaitForSeconds(dashDuration);

        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        isDashing = false;
        animator.SetBool("isDashing", false);
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(1f);
        canDash = true;
    }

    IEnumerator Dodge()
    {
        isDodging = true;
        canDodge = false;

        float dodgeDirection = isFacingRight ? -1f : 1f;
        Vector2 dodgeVelocity = new Vector2(dodgeDirection, 0f).normalized;

        Vector2 startPosition = rb.position;
        Vector2 targetPosition = startPosition + dodgeVelocity * dodgeDistance;

        float startTime = Time.time;
        while (Time.time < startTime + dodgeDuration)
        {
            float t = (Time.time - startTime) / dodgeDuration;
            rb.MovePosition(Vector2.Lerp(startPosition, targetPosition, t));
            yield return null;
        }

        isDodging = false;
        yield return new WaitForSeconds(1f);
        canDodge = true;
    }
}
