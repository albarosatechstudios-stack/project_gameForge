using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Look Settings")]
    [SerializeField] public float mouseSensitivity; // Sensibilità del mouse
    [SerializeField] private Transform cameraFollowTarget; // Assegna qui il transform che la telecamera deve seguire/guardare

    private PlayerControlsimputactions playerControls;
    private Vector2 currentMovementInput;
    private Vector3 currentMovement;
    private Vector2 currentLookInput;

    private CharacterController characterController;

    private float xRotation = 0f; // Rotazione sull'asse X (verticale) della telecamera/testa

    private void Awake()
    {
        playerControls = new PlayerControlsimputactions();
        characterController = GetComponent<CharacterController>();

        // Abilita l'azione di movimento e assegna il callback
        playerControls.Move.movement.started += OnMovementInput;
        playerControls.Move.movement.performed += OnMovementInput;
        playerControls.Move.movement.canceled += OnMovementInput;

        // Nuova: Abilita l'azione di look del mouse e assegna il callback
        playerControls.Move.look.started += OnLookInput;
        playerControls.Move.look.performed += OnLookInput;
        playerControls.Move.look.canceled += OnLookInput;

        // Opzionale: Blocca il cursore e nascondilo per un'esperienza FPS/TPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
        // Ripristina lo stato del cursore quando lo script è disabilitato
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        // Il movimento dovrebbe essere relativo alla direzione in cui il player sta guardando
        // non solo agli assi globali, quindi lo useremo in HandleMovement
    }

    private void OnLookInput(InputAction.CallbackContext context)
    {
        currentLookInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        HandleRotation(); // Gestisce la rotazione del player orizzontalmente con il mouse
        HandleMovement(); // Utilizza la nuova rotazione per il movimento
    }

    private void HandleMovement()
    {
        // Converti l'input 2D in un Vector3 locale (avanti/indietro, destra/sinistra)
        Vector3 localMovement = new Vector3(currentMovementInput.x, 0f, currentMovementInput.y);

        // Trasforma il movimento dal sistema di coordinate locale a quello globale del player
        // In questo modo, "avanti" è sempre dove il player sta guardando.
        Vector3 moveDirection = transform.TransformDirection(localMovement);

        // Applica il movimento al CharacterController
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        // Leggi l'input del mouse e applica la sensibilità
        float mouseX = currentLookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = currentLookInput.y * mouseSensitivity * Time.deltaTime;

        // Rotazione verticale (sull'asse X) per la telecamera o la "testa" del player
        // Accumuliamo la rotazione Y del mouse per evitare che superi i 90 gradi verso l'alto o il basso
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Limita la rotazione verticale a +/- 90 gradi

        // Applica la rotazione verticale al `cameraFollowTarget` (se impostato)
        // Questo permette alla telecamera di guardare su/giù senza ruotare l'intero player.
        if (cameraFollowTarget != null)
        {
            cameraFollowTarget.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        else
        {
            // Se non c'è un target specifico per la telecamera, applichiamo la rotazione verticale al player stesso
            // ATTENZIONE: Questo farà inclinare tutto il player su/giù, il che potrebbe non essere desiderato per un movimento FPS/TPS.
            // È meglio avere un oggetto figlio per la telecamera o la "testa".
            // transform.localRotation = Quaternion.Euler(xRotation, transform.localEulerAngles.y, 0f); // Non usare per FPS standard
        }


        // Rotazione orizzontale (sull'asse Y) per l'intero player
        // Ruota il GameObject del player attorno al suo asse Y globale
        transform.Rotate(Vector3.up * mouseX);
    }
}