using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerFirstPerson : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Cámara/Mirada")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float topClamp = 85f;
    [SerializeField] private float bottomClamp = -85f;

    [Header("Efectos de Audio")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip[] footstepSounds; // array que funciona para asignar varios audios
    [SerializeField] private float stepInterval = 0.5f; // tiempo entre cada paso
    [Range(0.1f, 1f)]
    [SerializeField] private float runstepLenghten = 0.7f;

    [Header("Head Bob (Balanceo de Cabeza)")]
    [SerializeField] private bool useHeadBob = true;
    [SerializeField] private float bobFrequency = 10f; // frecuencia del paso
    [SerializeField] private float bobAmount = 0.05f; // intensidad del movimiento

    [Header("Ajuste de Vista")]
    [SerializeField] private Transform headBone;

    private CharacterController controller;
    private AudioSource audioSource;
    private Animator animator;
    private InputManager inputManager;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float xRotation = 0f;
    private float defaultCameraY = 0f;
    private float timer = 0f;
    private float stepCycle = 0f; // funciona como acumulador de la frecuencia de pasos

    private bool isGrounded;
    private bool wasGrounded;
    private bool isSprinting;

    private void Start()
    {
        if (headBone != null)
        {
            headBone.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        }
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();

        // inicializa referenci InpuManager
        inputManager = new InputManager();

        if (cameraTransform != null)
        {
            defaultCameraY = cameraTransform.localPosition.y;
        }

        // opcional bloquear el cursor al centro del juego
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        // habilitamos el mapa de acciones "Player"
        inputManager.Player.Enable();
        inputManager.Player.Jump.performed += OnJump;
        inputManager.Player.Sprint.performed += ctx => isSprinting = true;
        inputManager.Player.Sprint.canceled += ctx => isSprinting = false;
        
    }
    private void OnDisable()
    {
        inputManager.Player.Sprint.performed -= ctx => isSprinting = true;
        inputManager.Player.Sprint.canceled -= ctx => isSprinting = false;
        inputManager.Player.Jump.performed -= OnJump;
        inputManager.Player.Disable();
    }

    private void Update()
    {
        // lectura continua de dtos tipo Vector2 (Polled Inputs)
        ReadInputs();
        // Procesa la rotación de cámara
        Look();
        // Procesa el desplazamiento del personaje
        Move();
        // cabeceo del jugador
        HandleHeadBobAndFootsteps();
        // actualiza las animaciones en cada frame
        UpdateAnimator();
    }

    private void ReadInputs()
    {
        // se leen directamente los valores que devuelve el InputSystem
        moveInput = inputManager.Player.Move.ReadValue<Vector2>();
        lookInput = inputManager.Player.Look.ReadValue<Vector2>();
    }

    private void Move()
    {
        isGrounded = controller.isGrounded;

        // detectar si acaba de aterrizar en este frame
        if (isGrounded && !wasGrounded)
        {
            PlaySound(landSound);
        }
        wasGrounded = isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float currentSpeed = (isSprinting && moveInput.y > 0) ? sprintSpeed : walkSpeed;
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Look()
    {
        // rotación horizontal del jugador
        float mouseX = lookInput.x * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        // rotación vertical de la cámara
        float mouseY = lookInput.y * mouseSensitivity;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, bottomClamp, topClamp);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            PlaySound(jumpSound);
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        // calcular magnitud de movimiento considerando si corre
        float speedMultiplier = (isSprinting && moveInput.y > 0) ? 1f : 0.5f;
        float forwardAmount = Mathf.Abs(moveInput.y) * speedMultiplier;
        //float forwardAmount = moveInput.y * speedMultiplier;
        float turnAmount = moveInput.x * speedMultiplier;

        // enviar parámetros al Animator Controller (mismos nombres que en ThirdPersonCharacter
        animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
        animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
        animator.SetBool("OnGround", isGrounded);

        if (!isGrounded)
        {
            animator.SetFloat("Jump", velocity.y);
        }
    }

    private void HandleHeadBobAndFootsteps()
    {
        // Si no se está moviendo en el suelo, reseteamos contadores
        if (!isGrounded || moveInput.magnitude < 0.1f)
        {
            timer = 0f;
            if (cameraTransform != null)
            {
                float resetY = Mathf.Lerp(cameraTransform.localPosition.y, defaultCameraY, Time.deltaTime * 8f);
                cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, resetY, cameraTransform.localPosition.z);
            }
            return;
        }

        // 1. Lógica del Head Bob (movimiento de cámara)
        if (useHeadBob && cameraTransform != null)
        {
            float speedMultiplier = isSprinting ? 1.4f : 1f;
            timer += Time.deltaTime * bobFrequency * speedMultiplier;

            float newY = defaultCameraY + Mathf.Sin(timer) * bobAmount;
            cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, newY, cameraTransform.localPosition.z);
        }

        // 2. Lógica de Pasos (Footsteps)
        float currentStepInterval = (isSprinting && moveInput.y > 0) ? (stepInterval * runstepLenghten) : stepInterval;
        stepCycle += Time.deltaTime;

        if (stepCycle >= currentStepInterval)
        {
            stepCycle = 0f;
            PlayFootstepAudio();
        }
    }

    private void PlayFootstepAudio()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;

        // selecciona un índice aleatorio de las pistas de pisadas
        int n = Random.Range(0, footstepSounds.Length);
        audioSource.PlayOneShot(footstepSounds[n]);

        // para evitar repetición de pista
        AudioClip selectedClip = footstepSounds[n];
        footstepSounds[n] = footstepSounds[0];
        footstepSounds[0] = selectedClip;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}