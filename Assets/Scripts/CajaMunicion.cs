using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CajaMunicion : MonoBehaviour
{
    [Header("Ajustes de Suministro")]
    public int capacidadCargadores = 10;
    public int cargadoresPorInteraccion = 2;
    public float tiempoRecarga = 15f;

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
    }

    private void OnEnable()
    {
        //Suscripcion a la accion de interaccion general del juego
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
        // Solo responde si el jugador esta dentro del area, la caja esta activa y tiene suministros
        if (jugadorEnZona && estaActiva && capacidadActual > 0) IntentarSuministro();
    }
    void Update()
    {
        // 1. Enfriamiento y regeneración de la caja[cite: 3]
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

        // 2. Interacción Híbrida mediante Input System[cite: 3]
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
        // Detecta la colisión espacial de la zona invisible[cite: 3]
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

        // Búsqueda estructural: El arma suele ser un hijo de la cámara o del cuerpo del jugador
        Ammo scriptAmmo = jugadorReferencia.GetComponentInChildren<Ammo>();

        // Prevención de errores si el jugador interactúa con las manos vacías o un arma cuerpo a cuerpo
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

        // Negociación de recursos
        int suministroReal = Mathf.Min(cargadoresPorInteraccion, cargadoresFaltantes);
        suministroReal = Mathf.Min(suministroReal, capacidadActual);

        capacidadActual -= suministroReal;

        // Inyección segura al arma
        scriptAmmo.RecibirCargadores(suministroReal);
        Debug.Log($"Suministrados {suministroReal} cargadores. Reserva en caja: {capacidadActual}");

        if (capacidadActual <= 0)
        {
            estaActiva = false;
            temporizador = tiempoRecarga;
            ActualizarBarraVisual(0f);
        }
    }

    // Motor de dibujado de la barra con fuente monoespaciada[cite: 3]
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