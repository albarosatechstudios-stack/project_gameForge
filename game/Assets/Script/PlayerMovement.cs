using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Look Settings")]
    public float mouseSensitivity = 10f;
    [SerializeField] private Transform cameraFollowTarget; // La telecamera o la "testa"

    private PlayerControlsimputactions playerControls;
    private CharacterController characterController;

    private Vector2 currentMovementInput;
    private Vector2 currentLookInput;
    private float verticalVelocity = 0;
    private float xRotation = 0f;

    public float forzaRepulsiva = 0f; // Mantenuta come da tuo script originale

    private void Awake()
    {
        playerControls = new PlayerControlsimputactions();
        characterController = GetComponent<CharacterController>();

        // Setup Input Movimento
        playerControls.Move.movement.performed += ctx => currentMovementInput = ctx.ReadValue<Vector2>();
        playerControls.Move.movement.canceled += ctx => currentMovementInput = Vector2.zero;

        // Setup Input Mouse
        playerControls.Move.look.performed += ctx => currentLookInput = ctx.ReadValue<Vector2>();
        playerControls.Move.look.canceled += ctx => currentLookInput = Vector2.zero;

        // Blocco cursore
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (forzaRepulsiva == 0f) forzaRepulsiva = 0.1f;
    }

    private void OnEnable() => playerControls.Enable();
    private void OnDisable()
    {
        playerControls.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Movimento orizzontale
        Vector3 localMovement = new Vector3(currentMovementInput.x, 0f, currentMovementInput.y);
        Vector3 moveDirection = transform.TransformDirection(localMovement);

        // Gravità
        if (characterController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // Piccola forza costante per tenerlo a terra
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        moveDirection.y = verticalVelocity;

        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        float mouseX = currentLookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = currentLookInput.y * mouseSensitivity * Time.deltaTime;

        // Rotazione verticale (Camera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (cameraFollowTarget != null)
        {
            cameraFollowTarget.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        else
        {
            // Fallback se non hai assegnato la camera, ruota il player (non ottimale per FPS)
            transform.localRotation = Quaternion.Euler(xRotation, transform.localEulerAngles.y, 0f);
        }

        // Rotazione orizzontale (Corpo Player)
        transform.Rotate(Vector3.up * mouseX);
    }
}