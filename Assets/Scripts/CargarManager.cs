using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CargarManager : MonoBehaviour
{
    [Header("Textos de los Slots (Arrastrar desde Inspector)")]
    public TextMeshProUGUI textoSlot1;
    public TextMeshProUGUI textoSlot2;
    public TextMeshProUGUI textoSlot3;

    [Header("Feedback Visual (Opcional)")]
    public TextMeshProUGUI textoEstado;

    [Header("Configuración de Escena")]
    // IMPORTANTE: Asegúrate de que diga "main" o el nombre exacto de tu escena de juego
    public string nombreEscenaJuego = "main";

    private int slotSeleccionado = -1;

    void Start()
    {
        ActualizarPantalla();
    }

    // --- 1. SELECCIÓN DE SLOT ---
    public void SeleccionarSlot(int numeroSlot)
    {
        // Solo permite seleccionar si el disco duro detecta que hay una partida guardada ahí
        if (PlayerPrefs.HasKey("Slot" + numeroSlot + "_Tiempo"))
        {
            slotSeleccionado = numeroSlot;
            if (textoEstado != null) textoEstado.text = $">> SLOT {numeroSlot} SELECCIONADO <<";
        }
        else
        {
            slotSeleccionado = -1;
            if (textoEstado != null) textoEstado.text = "ERROR: EL SLOT ESTÁ VACÍO";
        }
    }

    // --- 2. ACCIÓN DE CARGAR ---
    public void ClicCargarPartida()
    {
        if (slotSeleccionado == -1)
        {
            if (textoEstado != null) textoEstado.text = "SELECCIONA UNA PARTIDA VÁLIDA";
            return;
        }

        // 1. Le dejamos un "post-it" al nivel avisándole qué partida debe leer al nacer
        PlayerPrefs.SetInt("SlotAIniciar", slotSeleccionado);
        PlayerPrefs.Save();

        // 2. Cargamos la escena 3D
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    // --- 3. NUEVA PARTIDA (Para tu botón del Menú Principal) ---
    public void ClicNuevaPartida()
    {
        // Le avisamos al motor que ignore los guardados y arranque de cero
        PlayerPrefs.SetInt("SlotAIniciar", -1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    // --- 4. DIBUJADO VISUAL ---
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