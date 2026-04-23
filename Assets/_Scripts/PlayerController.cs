using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 8f;
    public float gravity = -20f;

    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Input Actions")]
    [Tooltip("Requires a Vector2 action (e.g., WASD/Left Stick)")]
    public InputActionReference moveAction;
    [Tooltip("Requires a Button action (e.g., Spacebar/South Button)")]
    public InputActionReference jumpAction;
    [Tooltip("Requires a Button action (e.g., Left Shift/Right Trigger)")]
    public InputActionReference dashAction;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector2 moveInput;

    private bool isDashing = false;
    private float dashTime;
    private float lastDashTime;

    [Header("Realm Swap Settings")]
    [Tooltip("Requires a Button action (e.g., Y/Triangle)")]
    public InputActionReference swapAction;
    public GameObject variantA;
    public GameObject variantB;
    public GameObject blackAndWhiteVolume;
    
    private bool isAlternateRealm = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void OnEnable()
    {
        // Enable the input actions
        moveAction.action.Enable();
        jumpAction.action.Enable();
        dashAction.action.Enable();

        // Subscribe to Jump and Dash events
        jumpAction.action.performed += OnJump;
        dashAction.action.performed += OnDash;

        // Swap characters
        swapAction.action.Enable();
        swapAction.action.performed += OnSwap;
    }

    void OnDisable()
    {
        // Disable the input actions to prevent memory leaks
        moveAction.action.Disable();
        jumpAction.action.Disable();
        dashAction.action.Disable();

        // Unsubscribe from events
        jumpAction.action.performed -= OnJump;
        dashAction.action.performed -= OnDash;

        // Swap
        swapAction.action.Disable();
        swapAction.action.performed -= OnSwap;
    }

    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // 1. Handle Dash Override
        if (isDashing)
        {
            if (Time.time < dashTime + dashDuration)
            {
                // Force the player forward while dashing
                controller.Move(transform.forward * dashSpeed * Time.deltaTime);
                return; // Skip normal movement while dashing
            }
            else
            {
                isDashing = false;
            }
        }

        // 2. Normal Movement
        moveInput = moveAction.action.ReadValue<Vector2>();
        
        // Map 2D input (X/Y) to 3D world space (X/Z) for top-down perspective
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        // Rotate the capsule to face the movement direction
        if (moveDirection != Vector3.zero)
        {
            gameObject.transform.forward = moveDirection;
        }

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // 3. Gravity & Ground Check
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small constant downward force to keep them snapped to the floor
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (controller.isGrounded && !isDashing)
        {
            velocity.y = jumpForce;
        }
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (Time.time > lastDashTime + dashCooldown && !isDashing)
        {
            // Optional: If you want to dash in the direction you are pointing the stick, 
            // rather than the direction the capsule is currently facing:
            if (moveInput != Vector2.zero)
            {
                transform.forward = new Vector3(moveInput.x, 0f, moveInput.y);
            }
            
            isDashing = true;
            dashTime = Time.time;
            lastDashTime = Time.time;
            
            velocity.y = 0f; // Neutralize gravity/jump momentum during the dash
        }
    }

    private void OnSwap(InputAction.CallbackContext context)
    {
        // Toggle the boolean
        isAlternateRealm = !isAlternateRealm;

        // Toggle the visual models
        if (variantA != null) variantA.SetActive(!isAlternateRealm);
        if (variantB != null) variantB.SetActive(isAlternateRealm);

        // Toggle the black and white screen effect
        if (blackAndWhiteVolume != null) blackAndWhiteVolume.SetActive(isAlternateRealm);
    }
}