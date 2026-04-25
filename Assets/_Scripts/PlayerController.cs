using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 8f;
    public float gravity = -20f;
    
    [Header("Camera Orbit Settings")]
    [Tooltip("Assign an Empty GameObject here. It should NOT be a child of the Player.")]
    public Transform cameraAnchor;
    public float cameraRotationSpeed = 0.5f;

    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference dashAction;
    public InputActionReference swapAction;

    [Header("Realm Swap Settings")]
    public GameObject variantA;
    public GameObject variantB;
    public GameObject blackAndWhiteVolume;
    
    private CharacterController controller;
    private Vector3 velocity;
    private Vector2 moveInput;

    private bool isDashing = false;
    private float dashTime;
    private float lastDashTime;
    private bool isAlternateRealm = false;

    private Camera mainCamera;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;
    }

    void EnableMovement()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        dashAction.action.Enable();
        jumpAction.action.performed += OnJump;
        dashAction.action.performed += OnDash;
        swapAction.action.Enable();
        swapAction.action.performed += OnSwap;
    }

    void OnEnable() => EnableMovement();

    void DisableMovement()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        dashAction.action.Disable();
        jumpAction.action.performed -= OnJump;
        dashAction.action.performed -= OnDash;
        swapAction.action.Disable();
        swapAction.action.performed -= OnSwap;
    }

    void OnDisable() => DisableMovement();

    void Update()
    {
        if(controller.enabled)
        {
            HandleCameraOrbit();
            HandleMovement();
        }
    }

    private void HandleCameraOrbit()
    {
        if (cameraAnchor != null)
        {
            // 1. Keep the anchor snapped to the player's feet
            cameraAnchor.position = transform.position;

            // 2. Spin the anchor when holding right-click
            if (Mouse.current != null && Mouse.current.rightButton.isPressed)
            {
                float mouseX = Mouse.current.delta.x.ReadValue();
                cameraAnchor.Rotate(0f, mouseX * cameraRotationSpeed, 0f, Space.World);
            }
        }
    }

    private void HandleMovement()
    {
        if (isDashing)
        {
            if (Time.time < dashTime + dashDuration)
            {
                controller.Move(transform.forward * dashSpeed * Time.deltaTime);
                return; 
            }
            else isDashing = false;
        }

        moveInput = moveAction.action.ReadValue<Vector2>();
        Vector3 moveDirection = Vector3.zero;

        // Calculate movement strictly relative to where the camera is looking
        if (mainCamera != null)
        {
            Vector3 forward = mainCamera.transform.forward;
            Vector3 right = mainCamera.transform.right;
            
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            moveDirection = forward * moveInput.y + right * moveInput.x;
        }

        // Rotate the character to face the direction they are walking
        if (moveDirection != Vector3.zero)
        {
            gameObject.transform.forward = moveDirection;
        }

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f; 
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (controller.isGrounded && !isDashing) velocity.y = jumpForce;
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (Time.time > lastDashTime + dashCooldown && !isDashing)
        {
            isDashing = true;
            dashTime = Time.time;
            lastDashTime = Time.time;
            velocity.y = 0f; 
        }
    }

    private void OnSwap(InputAction.CallbackContext context) => SwitchWorlds();

    void SwitchWorlds()
    {
        isAlternateRealm = !isAlternateRealm;
        if(isAlternateRealm)
        {
            //Level 1 World switch triggers
            DancingPuzzle.instance.HideSteps();
            DancingStatue.instance.Dance();

            //Level 2 World switch triggers
            // StatueController.instance.EnableAllStatues();
            // RabbitPathwaysManager.instance.DisableWalls();
        }
        else
        {
            //Level 1 world switch triggers
            DancingPuzzle.instance.RevealSteps();
            DancingStatue.instance.StopDancing();

            //Level 2 World switch triggers
            // StatueController.instance.DisableAllStatues();
            // RabbitPathwaysManager.instance.EnableWalls();
        }

        if (variantA != null) variantA.SetActive(!isAlternateRealm);
        if (variantB != null) variantB.SetActive(isAlternateRealm);
        if (blackAndWhiteVolume != null) blackAndWhiteVolume.SetActive(isAlternateRealm);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("StatueAggroZone")) other.transform.parent.GetComponent<StatueMovement>().SetStatueTarget(transform);
        if(other.CompareTag("StatueHitZone"))
        {
            other.transform.parent.GetComponent<StatueMovement>().SetStatueTarget(null);
            DisableMovement();
            controller.enabled = false;
            transform.position = StatueController.instance.resetPosition.position;
            controller.enabled = true;
            SwitchWorlds();
            EnableMovement();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("StatueAggroZone")) other.transform.parent.GetComponent<StatueMovement>().SetStatueTarget(null);
    }
}