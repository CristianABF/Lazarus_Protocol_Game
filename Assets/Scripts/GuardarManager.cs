using UnityEngine;
using TMPro;

public class GuardarManager : MonoBehaviour
{
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
        if (slotSeleccionado == -1)
        {
            if (textoEstado != null) textoEstado.text = "ERROR: SELECCIONA UN SLOT PRIMERO";
            return;
        }

        // Guardar la Meta-Progresión y Economía actual
        int saludActual = 100;
        int minutosSobrevividos = 0;
        int chatarraActual = 0;

        if (ControladorPartida.Instancia != null)
        {
            saludActual = Mathf.RoundToInt(ControladorPartida.Instancia.saludPermanenteNucleo);
            minutosSobrevividos = Mathf.FloorToInt(Time.timeSinceLevelLoad / 60f);
            chatarraActual = ControladorPartida.Instancia.creditosActuales;

            PlayerPrefs.SetInt("Slot" + slotSeleccionado + "_Chatarra", chatarraActual);
        }

        PlayerPrefs.SetInt("Slot" + slotSeleccionado + "_Tiempo", minutosSobrevividos);
        PlayerPrefs.SetInt("Slot" + slotSeleccionado + "_Salud", saludActual);

        // (Posición de Aris en el mapa)
        if (transformJugador != null)
        {
            PlayerPrefs.SetFloat("Slot" + slotSeleccionado + "_PosX", transformJugador.position.x);
            PlayerPrefs.SetFloat("Slot" + slotSeleccionado + "_PosY", transformJugador.position.y);
            PlayerPrefs.SetFloat("Slot" + slotSeleccionado + "_PosZ", transformJugador.position.z);

            PlayerPrefs.SetFloat("Slot" + slotSeleccionado + "_RotY", transformJugador.eulerAngles.y);
        }

        // (Inventario)
        if (scriptAgarre != null)
        {
            GameObject objetoEnMano = scriptAgarre.GetHeldObject();
            if (objetoEnMano != null)
            {
                // Guarda el nombre del objeto para reequiparlo luego
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
            int tiempo = PlayerPrefs.GetInt("Slot" + numeroSlot + "_Tiempo");
            int salud = PlayerPrefs.GetInt("Slot" + numeroSlot + "_Salud");

            textoSlot.text = $"Tiempo: {tiempo} MIN\nSalud: {salud}%";
        }
        else
        {
            textoSlot.text = "SLOT VACÍO\n";
        }
    }
}