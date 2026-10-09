using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Cámara / Mirada")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float topClamp = 85f;
    [SerializeField] private float bottomClamp = -85f;

    [Header("Animador de los Brazos")]
    [SerializeField] private Animator armsAnimator;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float xRotation = 0f;

    private bool isGrounded;
    private bool isSprinting;

    // Hash de animación optimizado
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int ReloadHash = Animator.StringToHash("Reload");
    private static readonly int HasWeaponHash = Animator.StringToHash("HasWeapon");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (armsAnimator == null)
        {
            armsAnimator = GetComponentInChildren<Animator>();
        }
    }

    private void OnEnable()
    {
        InputController.Input.Player.Enable();

        InputController.Input.Player.Jump.performed += OnJump;
        InputController.Input.Player.Sprint.performed += OnSprintStart;
        InputController.Input.Player.Sprint.canceled += OnSprintCanceled;
        InputController.Input.Player.Reload.performed += OnReload;
    }

    private void OnDisable()
    {
        if (InputController.Input != null)
        {
            InputController.Input.Player.Jump.performed -= OnJump;
            InputController.Input.Player.Sprint.performed -= OnSprintStart;
            InputController.Input.Player.Sprint.canceled -= OnSprintCanceled;
            InputController.Input.Player.Reload.performed -= OnReload;
        }
    }

    private void OnSprintStart(InputAction.CallbackContext context) => isSprinting = true;
    private void OnSprintCanceled(InputAction.CallbackContext context) => isSprinting = false;

    private void OnReload(InputAction.CallbackContext context)
    {
        if (PauseControl.isPaused) return;

        // Dispara el trigger de recarga en el Animator (si existe)
        if (armsAnimator != null) armsAnimator.SetTrigger(ReloadHash);

        // Ejecuta la recarga en el arma que el jugador tenga equipada
        Gun currentGun = GetComponentInChildren<Gun>();
        if (currentGun != null) currentGun.Reload();
    }
    private void Update()
    {
        ReadInputs();
        Look();
        Move();
        UpdateAnimator();
    }

    private void ReadInputs()
    {
        moveInput = InputController.Input.Player.Move.ReadValue<Vector2>();
        lookInput = InputController.Input.Player.Look.ReadValue<Vector2>();
    }

    private void Look()
    {
        // Rotación horizontal (gira todo el cuerpo y los brazos)
        float mouseX = lookInput.x * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        // Rotación vertical (solo inclina la cámara y los brazos adjuntos)
        float mouseY = lookInput.y * mouseSensitivity;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, bottomClamp, topClamp);

        if (cameraHolder != null)
        {
            cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    private void Move()
    {
        isGrounded = controller.isGrounded;

        // Mantener al jugador pegado al suelo cuando toca superficie
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Selección de velocidad
        float currentSpeed = (isSprinting && moveInput.y > 0) ? sprintSpeed : walkSpeed;

        // Vector de dirección de movimiento relativo al jugador
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(currentSpeed * Time.deltaTime * moveDirection);

        // Aplicar Gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void UpdateAnimator()
    {
        if (armsAnimator == null) return;

        // Determinar el valor para el Blend Tree 1D (Idle = 0, Walk = 0.5, Run = 1.0)
        float targetSpeed = 0f;
        if (moveInput.magnitude > 0.1f)
        {
            targetSpeed = (isSprinting && moveInput.y > 0) ? 1.0f : 0.5f;
        }

        // Enviar parámetro suavizado al Animator
        armsAnimator.SetFloat(SpeedHash, targetSpeed, 0.1f, Time.deltaTime);

        // Detectar si el jugador tiene un arma en la mano
        Gun currentGun = GetComponentInChildren<Gun>();
        armsAnimator.SetBool(HasWeaponHash, currentGun != null);
    }
}