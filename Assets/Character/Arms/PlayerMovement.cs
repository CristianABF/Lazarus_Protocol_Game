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

    [Header("Modelo de Brazos & Balanceo")]
    [Tooltip("Arrastra aquí el objeto PlayerArms_Model")]
    [SerializeField] private Transform armsModelTransform;
    [SerializeField] private bool enableBobbing = true;
    [Tooltip("Frecuencia del balanceo cuando está quieto (Idle)")]
    [SerializeField] private float idleBobFrequency = 2.5f;
    [Tooltip("Amplitud del balanceo cuando está quieto")]
    [SerializeField] private float idleBobAmount = 0.008f;
    [Tooltip("Frecuencia del balanceo al caminar")]
    [SerializeField] private float walkBobFrequency = 10f;
    [Tooltip("Amplitud del balanceo al caminar (más exagerado)")]
    [SerializeField] private float walkBobAmount = 0.04f;
    [Tooltip("Velocidad de suavizado del balanceo")]
    [SerializeField] private float bobSmoothness = 10f;

    [Header("Animador de los Brazos")]
    [SerializeField] private Animator armsAnimator;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float xRotation = 0f;

    private bool isGrounded;
    private bool isSprinting;

    // Control de disparo y balanceo
    private bool isShooting;
    private float bobTimer;
    private Vector3 defaultArmsPos;

    // Hash de animación optimizado
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int ReloadHash = Animator.StringToHash("Reload");
    private static readonly int HasWeaponHash = Animator.StringToHash("HasWeapon");
    private static readonly int HasWrenchHash = Animator.StringToHash("HasWrench");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (armsAnimator == null)
        {
            armsAnimator = GetComponentInChildren<Animator>();
        }

        // Si no asignaste el Transform de los brazos en el Inspector, busca el padre del Animator
        if (armsModelTransform == null && armsAnimator != null)
        {
            armsModelTransform = armsAnimator.transform.parent != null ? armsAnimator.transform.parent : armsAnimator.transform;
        }

        if (armsModelTransform != null)
        {
            defaultArmsPos = armsModelTransform.localPosition;
        }
    }

    void Start()
    {
        LockCursor();

        if (ControladorPartida.Instancia != null)
        {
            walkSpeed *= (1f + ControladorPartida.Instancia.velocidadCaminarExtra);
            sprintSpeed *= (1f + ControladorPartida.Instancia.velMaximaCaminarExtra);

            Debug.Log($"Velocidad calibrada. Caminar: {walkSpeed} | Correr: {sprintSpeed}");
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            LockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        InputController.Input.Player.Enable();

        InputController.Input.Player.Jump.performed += OnJump;
        InputController.Input.Player.Sprint.performed += OnSprintStart;
        InputController.Input.Player.Sprint.canceled += OnSprintCanceled;
        InputController.Input.Player.Reload.performed += OnReload;

        // Entradas de disparo
        InputController.Input.Player.Shoot.performed += OnShootStart;
        InputController.Input.Player.Shoot.canceled += OnShootCanceled;
    }

    private void OnDisable()
    {
        if (InputController.Input != null)
        {
            InputController.Input.Player.Jump.performed -= OnJump;
            InputController.Input.Player.Sprint.performed -= OnSprintStart;
            InputController.Input.Player.Sprint.canceled -= OnSprintCanceled;
            InputController.Input.Player.Reload.performed -= OnReload;

            InputController.Input.Player.Shoot.performed -= OnShootStart;
            InputController.Input.Player.Shoot.canceled -= OnShootCanceled;
        }
    }

    private void OnSprintStart(InputAction.CallbackContext context) => isSprinting = true;
    private void OnSprintCanceled(InputAction.CallbackContext context) => isSprinting = false;

    private void OnShootStart(InputAction.CallbackContext context) => isShooting = true;
    private void OnShootCanceled(InputAction.CallbackContext context) => isShooting = false;

    private void OnReload(InputAction.CallbackContext context)
    {
        if (PauseControl.isPaused) return;

        if (armsAnimator != null) armsAnimator.SetTrigger(ReloadHash);

        Gun currentGun = GetComponentInChildren<Gun>();
        if (currentGun != null) currentGun.Reload();
    }

    private void Update()
    {
        ReadInputs();
        Look();
        Move();
        HandleArmsBobbing();
        UpdateAnimator();
    }

    private void ReadInputs()
    {
        moveInput = InputController.Input.Player.Move.ReadValue<Vector2>();
        lookInput = InputController.Input.Player.Look.ReadValue<Vector2>();
    }

    private void Look()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

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

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float currentSpeed = (isSprinting && moveInput.y > 0) ? sprintSpeed : walkSpeed;

        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(currentSpeed * Time.deltaTime * moveDirection);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleArmsBobbing()
    {
        if (!enableBobbing || armsModelTransform == null) return;

        // Si se mantiene presionado disparar o está en el aire, se reestablece la posición original de los brazos
        if (isShooting || !isGrounded)
        {
            armsModelTransform.localPosition = Vector3.Lerp(armsModelTransform.localPosition, defaultArmsPos, Time.deltaTime * bobSmoothness);
            return;
        }

        bool isMoving = moveInput.magnitude > 0.1f;

        float frequency = isMoving ? walkBobFrequency : idleBobFrequency;
        float amount = isMoving ? walkBobAmount : idleBobAmount;

        if (isMoving && isSprinting && moveInput.y > 0)
        {
            frequency *= 1.3f;
            amount *= 1.25f;
        }

        bobTimer += Time.deltaTime * frequency;

        // Onda en forma de ocho vertical/horizontal aplicada exclusivamente al transform local del modelo de brazos
        float newY = defaultArmsPos.y + (Mathf.Sin(bobTimer) * amount);
        float newX = defaultArmsPos.x + (Mathf.Cos(bobTimer * 0.5f) * amount * 0.6f);

        Vector3 targetPosition = new Vector3(newX, newY, defaultArmsPos.z);
        armsModelTransform.localPosition = Vector3.Lerp(armsModelTransform.localPosition, targetPosition, Time.deltaTime * bobSmoothness);
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

        float targetSpeed = 0f;
        if (moveInput.magnitude > 0.1f)
        {
            targetSpeed = (isSprinting && moveInput.y > 0) ? 1.0f : 0.5f;
        }

        armsAnimator.SetFloat(SpeedHash, targetSpeed, 0.1f, Time.deltaTime);

        Gun currentGun = GetComponentInChildren<Gun>();
        armsAnimator.SetBool(HasWeaponHash, currentGun != null);

        WrenchBuilder currentWrench = GetComponentInChildren<WrenchBuilder>();
        armsAnimator.SetBool(HasWrenchHash, currentWrench != null && currentWrench.gameObject.activeInHierarchy);
    }
}