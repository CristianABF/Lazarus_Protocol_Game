using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnlazadorUI : MonoBehaviour
{
    [Header("Referencias de Paneles")]
    public GameObject panelLazaroNuevo;
    public GameObject panelGameOverNuevo;
    public GameObject panelMejorasNuevo;
    public TextMeshProUGUI textoEstadisticasNuevo;
    public TextMeshProUGUI textoSaldoBilletera;

    [Header("Botones a Reconectar Dinámicamente")]
    public Button btnActivarLazaro;
    public Button btnRendirse;
    public Button btnComenzarAsedio;

    // NUEVO: El botón final que vive en el panel de Game Over
    public Button btnVolverMenu;

    private string nombreEscenaMenu = "Menu_principal";

    private void Start()
    {
        if (ControladorPartida.Instancia != null)
        {
            // 1. Reconectar las variables visuales
            ControladorPartida.Instancia.pantallaProtocoloLazaro = panelLazaroNuevo;
            ControladorPartida.Instancia.pantallaGameOver = panelGameOverNuevo;
            ControladorPartida.Instancia.panelMejoras = panelMejorasNuevo;
            ControladorPartida.Instancia.textoEstadisticasGameOver = textoEstadisticasNuevo;

            if (textoSaldoBilletera != null)
            {
                textoSaldoBilletera.text = $"CRÉDITOS: {ControladorPartida.Instancia.creditosActuales} PTS";
            }

            // 2. Reconectar las vías de escape
            if (btnActivarLazaro != null)
            {
                btnActivarLazaro.onClick.RemoveAllListeners();
                btnActivarLazaro.onClick.AddListener(ControladorPartida.Instancia.AbrirTiendaMejoras);
            }

            if (btnRendirse != null)
            {
                btnRendirse.onClick.RemoveAllListeners();
                // CORRECCIÓN: Rendirse ahora solo abre la pantalla de estadísticas
                btnRendirse.onClick.AddListener(ControladorPartida.Instancia.RendirseYMostrarGameOver);
            }

            if (btnComenzarAsedio != null)
            {
                btnComenzarAsedio.onClick.RemoveAllListeners();
                btnComenzarAsedio.onClick.AddListener(ControladorPartida.Instancia.EjecutarReinicioTactico);
            }

            // NUEVO: El botón de Game Over asume la responsabilidad de cambiar de escena
            if (btnVolverMenu != null)
            {
                btnVolverMenu.onClick.RemoveAllListeners();
                btnVolverMenu.onClick.AddListener(() => ControladorPartida.Instancia.RegresarAlMenuPrincipal(nombreEscenaMenu));
            }

            Debug.Log("Enlazador UI: Flujo de derrota reconectado correctamente.");
        }
    }
}