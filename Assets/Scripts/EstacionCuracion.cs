using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class EstacionCuracion : MonoBehaviour
{
    [Header("Ajustes de Capacidad")]
    public float capacidadMaxima = 100f;
    public float cantidadPorInteraccion = 25f;
    public float tiempoRecarga = 10f;

    [Header("Configuración de Audio")]
    [Tooltip("AudioSource de la estación de curación.")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("AudioClip que sonará cuando la estación se quede sin salud y empiece a recargar.")]
    [SerializeField] private AudioClip audioSinSalud;

    [Header("Efectos Visuales (VFX)")]
    [Tooltip("Asigna aquí un Prefab de partículas. Se instanciará en el jugador cuando reciba curación.")]
    public GameObject vfxCuracionPrefab;
    [Tooltip("Desplazamiento para ajustar la altura o posición del VFX respecto al jugador.")]
    public Vector3 offsetVfx = new Vector3(0f, 1f, 0f);
    [Tooltip("Opcional: Si querés reproducir un ParticleSystem que forma parte de la estación.")]
    public ParticleSystem vfxEstacion;

    [Header("Interfaz Física (Arrastrar desde Hierarchy)")]
    public TMP_Text textoBarraRecarga;

    private float capacidadActual;
    private bool estaActiva = true;
    private float temporizador = 0f;
    private bool jugadorEnZona = false;

    private PlayerBehaviour scriptJugador;

    void Start()
    {
        capacidadActual = capacidadMaxima;
        ActualizarBarraVisual(1f);

        if (ControladorPartida.Instancia != null)
        {
            tiempoRecarga *= ControladorPartida.Instancia.factorRecargaEstaciones;
        }

        // Auto-asignación si no se arrastró en el Inspector
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        if (InputController.Input != null)
        {
            InputController.Input.Player.Interact.performed += OnInteract;
        }
    }

    private void OnDisable()
    {
        if (InputController.Input != null)
        {
            InputController.Input.Player.Interact.performed -= OnInteract;
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (jugadorEnZona && estaActiva && capacidadActual > 0)
        {
            IntentarCuracion();
        }
    }

    void Update()
    {
        if (!estaActiva)
        {
            temporizador -= Time.deltaTime;
            float progresoRecarga = 1f - (temporizador / tiempoRecarga);
            ActualizarBarraVisual(progresoRecarga);

            if (temporizador <= 0)
            {
                estaActiva = true;
                capacidadActual = capacidadMaxima;
                ActualizarBarraVisual(1f);
                Debug.Log("Estación reiniciada al 100%");
            }
        }

        if (jugadorEnZona && estaActiva && capacidadActual > 0)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                IntentarCuracion();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnZona = true;
            scriptJugador = other.GetComponent<PlayerBehaviour>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnZona = false;
            scriptJugador = null;
        }
    }

    private void IntentarCuracion()
    {
        if (scriptJugador == null) return;

        float vidaFaltante = scriptJugador.maxHealth - scriptJugador.playerHealth;

        if (vidaFaltante <= 0) return;

        float curacionReal = Mathf.Min(cantidadPorInteraccion, vidaFaltante);
        curacionReal = Mathf.Min(curacionReal, capacidadActual);

        capacidadActual -= curacionReal;
        scriptJugador.RecibirCuracion(curacionReal);

        ReproducirEfectoCuracion();

        if (capacidadActual <= 0)
        {
            estaActiva = false;
            temporizador = tiempoRecarga;
            ActualizarBarraVisual(0f);

            // Reproduce el audio de estación agotada
            ReproducirAudio(audioSinSalud);
        }
    }

    private void ReproducirEfectoCuracion()
    {
        if (vfxCuracionPrefab != null && scriptJugador != null)
        {
            Vector3 posicionConOffset = scriptJugador.transform.position + scriptJugador.transform.TransformDirection(offsetVfx);
            GameObject vfxInstance = Instantiate(vfxCuracionPrefab, posicionConOffset, scriptJugador.transform.rotation);
            vfxInstance.transform.SetParent(scriptJugador.transform);
            Destroy(vfxInstance, 2f);
        }

        if (vfxEstacion != null)
        {
            vfxEstacion.Play();
        }
    }

    private void ReproducirAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void ActualizarBarraVisual(float porcentaje)
    {
        if (textoBarraRecarga == null) return;

        int bloquesTotales = 10;
        int bloquesLlenos = Mathf.Clamp(Mathf.RoundToInt(porcentaje * bloquesTotales), 0, bloquesTotales);

        string barras = new string('█', bloquesLlenos) + new string('▒', bloquesTotales - bloquesLlenos);

        if (estaActiva)
        {
            textoBarraRecarga.text = $"<color=#00FF00>{barras}</color>";
        }
        else
        {
            textoBarraRecarga.text = $"<color=#FF0000>{barras}</color>";
        }
    }
}