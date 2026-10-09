using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // Obligatorio para controlar el texto 3D

public class EstacionCuracion : MonoBehaviour
{
    [Header("Ajustes de Capacidad")]
    public float capacidadMaxima = 100f;
    public float cantidadPorInteraccion = 25f;
    public float tiempoRecarga = 10f;

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
        ActualizarBarraVisual(1f); // 1f = 100% de capacidad al inicio
    }

    private void OnEnable()
    {
        // Suscripción a la acción de interacción general del juego
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

            // 1. Cálculo matemático riguroso del progreso (de 0 a 1)
            float progresoRecarga = 1f - (temporizador / tiempoRecarga);
            ActualizarBarraVisual(progresoRecarga);

            if (temporizador <= 0)
            {
                estaActiva = true;
                capacidadActual = capacidadMaxima;
                ActualizarBarraVisual(1f); // Vuelve al estado de barra llena
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

        if (vidaFaltante <= 0) return; // Rechaza si está al máximo

        float curacionReal = Mathf.Min(cantidadPorInteraccion, vidaFaltante);
        curacionReal = Mathf.Min(curacionReal, capacidadActual);

        capacidadActual -= curacionReal;
        scriptJugador.RecibirCuracion(curacionReal);

        // Si se vacía, la barra comienza a vaciarse visualmente a 0
        if (capacidadActual <= 0)
        {
            estaActiva = false;
            temporizador = tiempoRecarga;
            ActualizarBarraVisual(0f);
        }
    }

    // --- NUEVO: MOTOR DE DIBUJADO DE LA BARRA ---
    private void ActualizarBarraVisual(float porcentaje)
    {
        if (textoBarraRecarga == null) return;

        int bloquesTotales = 10;
        // Multiplica el porcentaje por 10 y redondea para saber cuántos cuadros pintar
        int bloquesLlenos = Mathf.Clamp(Mathf.RoundToInt(porcentaje * bloquesTotales), 0, bloquesTotales);

        // Construye el string combinando el bloque lleno (█) y el tramado (▒)
        string barras = new string('█', bloquesLlenos) + new string('▒', bloquesTotales - bloquesLlenos);

        // Aplica color verde si está lista para usarse, o rojo si está recargando
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