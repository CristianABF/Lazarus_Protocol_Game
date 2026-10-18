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

    [Header("Referencia de Salud")]
    public PlayerHealthBar hudSalud; // Arrastrar la barra de salud aquí

    [Header("Botones a Reconectar Dinámicamente")]
    public Button btnActivarLazaro;
    public Button btnRendirse;
    public Button btnComenzarAsedio;
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
                btnActivarLazaro.onClick.AddListener(() =>
                {
                    OcultarHUDVida();
                    ControladorPartida.Instancia.AbrirTiendaMejoras();
                });
            }

            if (btnRendirse != null)
            {
                btnRendirse.onClick.RemoveAllListeners();
                btnRendirse.onClick.AddListener(() =>
                {
                    OcultarHUDVida();
                    ControladorPartida.Instancia.RendirseYMostrarGameOver();
                });
            }

            if (btnComenzarAsedio != null)
            {
                btnComenzarAsedio.onClick.RemoveAllListeners();
                btnComenzarAsedio.onClick.AddListener(ControladorPartida.Instancia.EjecutarReinicioTactico);
            }

            if (btnVolverMenu != null)
            {
                btnVolverMenu.onClick.RemoveAllListeners();
                btnVolverMenu.onClick.AddListener(() => ControladorPartida.Instancia.RegresarAlMenuPrincipal(nombreEscenaMenu));
            }

            Debug.Log("Enlazador UI: Flujo de derrota reconectado correctamente.");
        }
    }

    private void Update()
    {
        // Detecta si cualquiera de los paneles especiales está activo para ocultar el HUD de salud
        if (hudSalud != null)
        {
            bool algunPanelActivo = (panelLazaroNuevo != null && panelLazaroNuevo.activeSelf) ||
                                   (panelGameOverNuevo != null && panelGameOverNuevo.activeSelf) ||
                                   (panelMejorasNuevo != null && panelMejorasNuevo.activeSelf);

            if (algunPanelActivo)
            {
                hudSalud.SetHUDVisible(false);
            }
        }
    }

    public void OcultarHUDVida()
    {
        if (hudSalud != null)
        {
            hudSalud.SetHUDVisible(false);
        }
    }
}