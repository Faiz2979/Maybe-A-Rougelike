using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float sprintSpeed = 14f;
    [SerializeField] private float crouchSpeed = 4f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float groundLine = 1f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isCrouched = false;
    private bool isGrounded = false;
    private bool facingRight = true;
    private bool isRunning = false;
    private bool isSprinting = false;
    private float currentSpeed;

    private PlayerInput playerInput;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component is missing!", this);
        }

        rb.freezeRotation = true;
        currentSpeed = walkSpeed; // Default kecepatan jalan
    }

    private void Start()
    {
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("Player");
        }
    }

    private void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.actions["Move"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            playerInput.actions["Move"].canceled += ctx => moveInput = Vector2.zero;

            playerInput.actions["Crouch"].performed += ctx => StartCrouch();
            playerInput.actions["Crouch"].canceled += ctx => StopCrouch();

            playerInput.actions["Jump"].performed += ctx => Jump();

            playerInput.actions["Sprint"].performed += ctx => StartSprint();
            playerInput.actions["Sprint"].canceled += ctx => StopSprint();
        }
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.actions["Move"].performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
            playerInput.actions["Move"].canceled -= ctx => moveInput = Vector2.zero;

            playerInput.actions["Crouch"].performed -= ctx => StartCrouch();
            playerInput.actions["Crouch"].canceled -= ctx => StopCrouch();

            playerInput.actions["Jump"].performed -= ctx => Jump();

            playerInput.actions["Sprint"].performed -= ctx => StartSprint();
            playerInput.actions["Sprint"].canceled -= ctx => StopSprint();
        }
    }

    private void Update()
    {
        CheckGround();
        MovePlayer();
        AnimationHandler();
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, groundLine, groundMask);
    }

    private void MovePlayer()
    {
        float moveDirection = moveInput.x;

        if (Mathf.Abs(moveDirection) > 0.1f)
        {
            if (isCrouched)
                currentSpeed = crouchSpeed;
            else if (isSprinting)
                currentSpeed = sprintSpeed;
            else if (isRunning)
                currentSpeed = runSpeed;
            else
                currentSpeed = walkSpeed; // Default jalan biasa

            rb.linearVelocity = new Vector2(moveDirection * currentSpeed, rb.linearVelocity.y);
            RotateCharacter(moveDirection);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void RotateCharacter(float moveDirection)
    {
        if (moveDirection > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveDirection < 0 && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void Jump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void StartCrouch()
    {
        isCrouched = true;
        animator.SetBool("Crouched", true);
    }

    private void StopCrouch()
    {
        isCrouched = false;
        animator.SetBool("Crouched", false);
    }

    private async void StartSprint()
    {
        isRunning = true;
        isSprinting = false;
        currentSpeed = runSpeed; // Awalnya hanya lari biasa
        Debug.Log("Started Running...");

        await Task.Delay(5000);

        if (playerInput.actions["Sprint"].IsPressed()) 
        {
            isSprinting = true;
            Debug.Log("Sprint Activated");
        }
    }

    private void StopSprint()
    {
        isRunning = false;
        isSprinting = false;
        currentSpeed = walkSpeed; // Kembali ke jalan biasa
        Debug.Log("Stopped Sprinting.");
    }

    private void AnimationHandler()
    {
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("isGrounded", isGrounded);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(groundCheck.position, Vector2.down * groundLine);
    }
}
