using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CajaMunicion : MonoBehaviour
{
    [Header("Ajustes de Suministro")]
    public int capacidadCargadores = 10;
    public int cargadoresPorInteraccion = 2;
    public float tiempoRecarga = 15f;

    [Header("Configuración de Audio")]
    [Tooltip("AudioSource de la caja de munición.")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("AudioClip que sonará cuando la caja quede vacía y comience a recargar.")]
    [SerializeField] private AudioClip audioSinMunicion;

    [Header("Interfaz Física (World Space)")]
    public TMP_Text textoBarraRecarga;

    private int capacidadActual;
    private bool estaActiva = true;
    private float temporizador = 0f;
    private bool jugadorEnZona = false;

    private GameObject jugadorReferencia;

    void Start()
    {
        capacidadActual = capacidadCargadores;
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
        InputController.Input.Player.Interact.performed += OnInteract;
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
        if (jugadorEnZona && estaActiva && capacidadActual > 0) IntentarSuministro();
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
                capacidadActual = capacidadCargadores;
                ActualizarBarraVisual(1f);
            }
        }

        if (jugadorEnZona && estaActiva && capacidadActual > 0)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                IntentarSuministro();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnZona = true;
            jugadorReferencia = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnZona = false;
            jugadorReferencia = null;
        }
    }

    private void IntentarSuministro()
    {
        if (jugadorReferencia == null) return;

        Ammo scriptAmmo = jugadorReferencia.GetComponentInChildren<Ammo>();

        if (scriptAmmo == null)
        {
            Debug.LogWarning("Interacción rechazada: El jugador no tiene un arma con munición equipada.");
            return;
        }

        int cargadoresFaltantes = scriptAmmo.GetMaxClips() - scriptAmmo.GetCurrentClips();

        if (cargadoresFaltantes <= 0)
        {
            Debug.Log("Munición de reserva al máximo. Rechazando interacción para no desperdiciar recursos.");
            return;
        }

        int suministroReal = Mathf.Min(cargadoresPorInteraccion, cargadoresFaltantes);
        suministroReal = Mathf.Min(suministroReal, capacidadActual);

        capacidadActual -= suministroReal;

        scriptAmmo.RecibirCargadores(suministroReal);
        Debug.Log($"Suministrados {suministroReal} cargadores. Reserva en caja: {capacidadActual}");

        if (capacidadActual <= 0)
        {
            estaActiva = false;
            temporizador = tiempoRecarga;
            ActualizarBarraVisual(0f);

            // Reproduce el audio de caja vacía
            ReproducirAudio(audioSinMunicion);
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
            textoBarraRecarga.text = $"<color=#FFD700>MUNICIÓN:\n{barras}</color>";
        }
        else
        {
            textoBarraRecarga.text = $"<color=#FF0000>RECARGANDO:\n{barras}</color>";
        }
    }
}