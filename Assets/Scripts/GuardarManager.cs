using UnityEngine;
using TMPro;

public class GuardarManager : MonoBehaviour
{
    // Bandera global: true durante el descanso y false al iniciar la oleada
    public static bool enDescanso = false;

    [Header("Textos de los Slots (Arrastrar desde Inspector)")]
    public TextMeshProUGUI textoSlot1;
    public TextMeshProUGUI textoSlot2;
    public TextMeshProUGUI textoSlot3;

    [Header("Feedback Visual (Opcional)")]
    public TextMeshProUGUI textoEstado;

    [Header("Referencias Físicas del Jugador")]
    public Transform transformJugador;
    public HoldObject scriptAgarre;

    private int slotSeleccionado = -1;

    void Start()
    {
        ActualizarPantalla();
    }

    // --- SELECCIÓN DE SLOT ---
    public void SeleccionarSlot(int numeroSlot)
    {
        slotSeleccionado = numeroSlot;
        if (textoEstado != null) textoEstado.text = $">> SLOT {numeroSlot} SELECCIONADO <<";
    }

    // --- ACCIÓN DE GUARDAR ---
    public void ClicGuardarPartida()
    {
        // 1. Validar que estemos en tiempo de descanso entre oleadas
        if (!enDescanso)
        {
            if (textoEstado != null) textoEstado.text = "ERROR: SOLO PUEDES GUARDAR ENTRE OLEADAS";
            return;
        }

        if (slotSeleccionado == -1)
        {
            if (textoEstado != null) textoEstado.text = "ERROR: SELECCIONA UN SLOT PRIMERO";
            return;
        }

        // 2. Guardar la Meta-Progresión y Economía actual
        int saludActual = 100;
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        int segundosTotales = waveManager != null
            ? Mathf.FloorToInt(waveManager.GetTiempoTranscurrido())
            : Mathf.FloorToInt(Time.timeSinceLevelLoad);
        int chatarraActual = 0;

        if (transformJugador != null)
        {
            InventarioJugador inv = transformJugador.GetComponent<InventarioJugador>();
            int chatarraWorkbench = inv != null ? inv.chatarraAcumulada : 0;
            PlayerPrefs.SetInt("Slot" + slotSeleccionado + "_ChatarraWorkbench", chatarraWorkbench);
        }

        if (ControladorPartida.Instancia != null)
        {
            saludActual = Mathf.RoundToInt(ControladorPartida.Instancia.saludPermanenteNucleo);
            chatarraActual = ControladorPartida.Instancia.creditosActuales;

            PlayerPrefs.SetInt("Slot" + slotSeleccionado + "_Chatarra", chatarraActual);
        }

        // Ahora guardamos el total de segundos para no perder precisión
        PlayerPrefs.SetInt("Slot" + slotSeleccionado + "_Tiempo", segundosTotales);
        PlayerPrefs.SetInt("Slot" + slotSeleccionado + "_Salud", saludActual);

        // 3. Posición de Aris en el mapa
        if (transformJugador != null)
        {
            PlayerPrefs.SetFloat("Slot" + slotSeleccionado + "_PosX", transformJugador.position.x);
            PlayerPrefs.SetFloat("Slot" + slotSeleccionado + "_PosY", transformJugador.position.y);
            PlayerPrefs.SetFloat("Slot" + slotSeleccionado + "_PosZ", transformJugador.position.z);

            PlayerPrefs.SetFloat("Slot" + slotSeleccionado + "_RotY", transformJugador.eulerAngles.y);
        }

        // 4. Inventario
        if (scriptAgarre != null)
        {
            GameObject objetoEnMano = scriptAgarre.GetHeldObject();
            if (objetoEnMano != null)
            {
                PlayerPrefs.SetString("Slot" + slotSeleccionado + "_ObjetoMano", objetoEnMano.name);
            }
            else
            {
                PlayerPrefs.SetString("Slot" + slotSeleccionado + "_ObjetoMano", "Vacio");
            }
        }

        PlayerPrefs.Save();

        if (textoEstado != null) textoEstado.text = $"PARTIDA GUARDADA EN SLOT {slotSeleccionado}";

        ActualizarPantalla();
    }

    // --- DIBUJADO VISUAL ---
    private void ActualizarPantalla()
    {
        DibujarDatosSlot(1, textoSlot1);
        DibujarDatosSlot(2, textoSlot2);
        DibujarDatosSlot(3, textoSlot3);
    }

    private void DibujarDatosSlot(int numeroSlot, TextMeshProUGUI textoSlot)
    {
        if (textoSlot == null) return;

        if (PlayerPrefs.HasKey("Slot" + numeroSlot + "_Tiempo"))
        {
            int segundosTotales = PlayerPrefs.GetInt("Slot" + numeroSlot + "_Tiempo");
            int salud = PlayerPrefs.GetInt("Slot" + numeroSlot + "_Salud");

            int minutos = segundosTotales / 60;
            int segundos = segundosTotales % 60;
            string tiempoFormateado = string.Format("{0:00}:{1:00}", minutos, segundos);

            textoSlot.text = $"Tiempo: {tiempoFormateado}\nSalud: {salud}%";
        }
        else
        {
            textoSlot.text = "SLOT VACÍO\n";
        }
    }
}