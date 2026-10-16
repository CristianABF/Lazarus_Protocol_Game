using UnityEngine;
using UnityEngine.UI; // Obligatorio para manipular botones por código
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

    // Ajusta el nombre exacto de tu escena de menú aquí si es distinto
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

            // 2. Reconectar las vías de escape de los botones
            if (btnActivarLazaro != null)
            {
                btnActivarLazaro.onClick.RemoveAllListeners(); // Purgar la basura del editor
                btnActivarLazaro.onClick.AddListener(ControladorPartida.Instancia.AbrirTiendaMejoras);
            }

            if (btnRendirse != null)
            {
                btnRendirse.onClick.RemoveAllListeners();
                btnRendirse.onClick.AddListener(() => ControladorPartida.Instancia.RegresarAlMenuPrincipal(nombreEscenaMenu));
            }

            if (btnComenzarAsedio != null)
            {
                btnComenzarAsedio.onClick.RemoveAllListeners();
                btnComenzarAsedio.onClick.AddListener(ControladorPartida.Instancia.EjecutarReinicioTactico);
            }

            Debug.Log("Enlazador UI: Paneles y botones reconectados al cerebro inmortal.");
        }
    }
}