using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

// Oyuncu karakterinin hareketini kontrol eden C# script'i
public class PlayerMovement : MonoBehaviour
{
    // Effect �zellikleri
    public ParticleSystem dust;

    // Hareket ve ziplama ozellikleri
    public float moveSpeed = 5f;

    // Yon kontrol degiskenleri
    private bool isFacingRight = true;

    // Animator bileseni
    private Animator animator;

    // Fiziksel ozellikleri yonetmek icin Rigidbody bileseni
    private Rigidbody2D rb;

    private ShiftController shiftController;
    private JumpController jumpController;

    [HideInInspector]public float rayDirection;
    [HideInInspector]public Vector2 movement;

    // Baslangic metodu - Oyun basladiginda bir kere �alisir
    void Start()
    {
        shiftController = GetComponent<ShiftController>();
        jumpController = GetComponent<JumpController>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Guncelleme metodu - Her karede bir kere calisir
    void Update()
    {
        // Kullanici girislerini kontrol et
        MovementInput();
        CheckRayDirection();
    }

    void MovementInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        Vector2 movement = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));

        // If the character is dashing, update the velocity based on dash speed
        if (shiftController.isDashing)
        {
            movement = rb.velocity.normalized * shiftController.dashSpeed;
        }
        // Update the rigidbody's velocity with the movement vector
        rb.velocity = movement;

        // Karakterin yuzunu cevir
        FlipCharacter(horizontalInput);

        if (jumpController.isWallSliding)
        {
            movement = new Vector2(Input.GetAxis("Horizontal") * 0f, rb.velocity.y);
            rb.velocity = movement;
        }
        CreateDust(horizontalInput);
    }

    // Karakteri cevirme metodu
    void FlipCharacter(float horizontalInput)
    {
        // Karakterin y�z�n� �evirme
        if ((horizontalInput < 0 && isFacingRight) || (horizontalInput > 0 && !isFacingRight))
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    void CheckRayDirection()
    {
        // Ray y�n�n� g�ncelle
        rayDirection = isFacingRight ? 1f : -1f;
    }
    void CreateDust(float horizontalInput)
    {
        if (Mathf.Abs(horizontalInput) > 0.1f && jumpController.isGrounded)
        {
            dust.Play();
        }
    }
}
