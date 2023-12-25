using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpController : MonoBehaviour
{
    //Ziplama ozellikleri
    public float jumpForceMin = 1f;
    public float jumpForceMax = 5f;
    public float maxJumpTime = 0.1f;

    // Ziplama kontrol degiskenleri
    private bool isGrounded;
    private bool isJumping;
    private float jumpTime;
    private bool hasJumped;
    private int doubleJumpCount;

    // �ift ziplama ozellikleri
    public int maxDoubleJumps = 1;
    public float doubleJumpForce = 5f;

    // Animator bileseni
    private Animator animator;

    //PlayerMovement bileseni
    public PlayerMovement playerMovement;

    //CapsuleCollider bileseni
    public CapsuleCollider2D capsuleCollider2d;

    //LayerMask Bileseni
    public LayerMask groundlayerMask;

    //Duvar kontrol degiskeni
    private bool isTouchingWall;

    // Baslangic metodu - Oyun basladiginda bir kere �alisir
    void Start()
    {
        animator = GetComponent<Animator>(); //Animator Caching
        capsuleCollider2d = GetComponent<CapsuleCollider2D> (); //CapsuleCollider Caching
    }

    void Update()
    {
        CheckJumpInput();
        CheckFalling();

    }
    void FixedUpdate()
    {
        CheckGrounded();
        CheckLedges();
    }

    void CheckGrounded()
    {
        // Karakterin yerde olup olmad���n� kontrol et
        RaycastHit2D raycastHit = Physics2D.Raycast(capsuleCollider2d.bounds.center, Vector2.down, capsuleCollider2d.bounds.extents.y + 0.2f, groundlayerMask);

        Color rayColor;

        if (raycastHit.collider != null)
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
        }

        Debug.DrawRay(capsuleCollider2d.bounds.center, Vector2.down * (capsuleCollider2d.bounds.extents.y + 0.2f), rayColor);
        isGrounded = raycastHit.collider != null;


        // Karakter yerdeyse, z�plama durumunu s�f�rla
        if (isGrounded)
        {
            hasJumped = false;
            doubleJumpCount = 0;
        }
        
    }
    

    void CheckJumpInput()
    {
        // "Jump" tu�una bas�ld���nda
        if (Input.GetButtonDown("Jump"))
        {
            // E�er yerdeyse
            if (isGrounded)
            {
                // Z�plama ba�lat
                isJumping = true;
                jumpTime = 0f;
                playerMovement.rb.velocity = new Vector2(playerMovement.rb.velocity.x, jumpForceMin);
                animator.SetBool("isJumping", true);
            }
            // Yerde de�ilse ve �ift z�plama kullan�labilirse
            else if (!hasJumped && doubleJumpCount < maxDoubleJumps)
            {
                // �ift z�plama ba�lat
                hasJumped = true;
                doubleJumpCount++;
                playerMovement.rb.velocity = new Vector2(playerMovement.rb.velocity.x, doubleJumpForce);
                animator.SetBool("isJumping", true);
            }
        }

        // "Jump" tu�una bas�l� tutuldu�u s�rece ve z�plama zaman s�n�r�na ula��lmam��sa
        if (Input.GetButton("Jump") && isJumping && jumpTime < maxJumpTime)
        {
            jumpTime += Time.deltaTime;
            float jumpForce = Mathf.Lerp(jumpForceMin, jumpForceMax, jumpTime / maxJumpTime); // Lineer Interpolasyon
            playerMovement.rb.velocity = new Vector2(playerMovement.rb.velocity.x, jumpForce);
        }
        else
        {
            if (isGrounded)
            {
                animator.SetBool("isJumping", false);
            }
        }

        // "Jump" tu�u b�rak�ld���nda
        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
            jumpTime = 0f;
            animator.SetBool("isJumping", false);
        }
    }
    void CheckLedges()
    {
        RaycastHit2D raycastHitTop = Physics2D.Raycast(new Vector2(capsuleCollider2d.bounds.center.x, capsuleCollider2d.bounds.max.y), Vector2.right * playerMovement.rayDirection, 1f, groundlayerMask);

        Color rayColorTop;

        if (raycastHitTop.collider != null)
        {
            rayColorTop = Color.green;
        }
        else
        {
            rayColorTop = Color.red;
        }

        Debug.DrawRay(new Vector2(capsuleCollider2d.bounds.center.x, capsuleCollider2d.bounds.max.y), Vector2.right * playerMovement.rayDirection * 1f, rayColorTop);

        RaycastHit2D raycastHitCenter = Physics2D.Raycast(new Vector2(capsuleCollider2d.bounds.center.x, capsuleCollider2d.bounds.center.y), Vector2.right * playerMovement.rayDirection, 1f, groundlayerMask);

        Color rayColorCenter;

        if (raycastHitCenter.collider != null)
        {
            rayColorCenter = Color.green;
        }
        else
        {
            rayColorCenter = Color.red;
        }

        Debug.DrawRay(new Vector2(capsuleCollider2d.bounds.center.x, capsuleCollider2d.bounds.center.y), Vector2.right * playerMovement.rayDirection * 1f, rayColorCenter);

        isTouchingWall = raycastHitCenter.collider != null;

        //Tepedeki ray bo�ta ve merkezdeki ray collidera de�iyorsa ledge climb yap
        if (raycastHitTop.collider == null && raycastHitCenter.collider != null && !isGrounded)
        {
            PerformLedgeClimb();
        }
    }
    void PerformLedgeClimb()
    {
        Vector2 ledgeClimbPosition = new Vector2(capsuleCollider2d.bounds.center.x + playerMovement.rayDirection, capsuleCollider2d.bounds.max.y);
        transform.position = ledgeClimbPosition;
    }
    void CheckFalling()
    {
        if (playerMovement.rb.velocity.y < 0f)
        {
            animator.SetBool("isFalling", true);
        }
        else
        {
            animator.SetBool("isFalling", false);
        }
    }
}
