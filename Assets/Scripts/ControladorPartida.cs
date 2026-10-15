using UnityEngine;
using UnityEngine.SceneManagement; // Obligatorio para recargar los niveles

public class ControladorPartida : MonoBehaviour
{
    public static ControladorPartida Instancia;

    [Header("Meta-Progresión de la Nave")]
    public float saludPermanenteNucleo = 100f;
    public float penalidadPorColapso = 15f;

    [Header("Pantallas de Fin de Partida")]
    public GameObject pantallaProtocoloLazaro;
    public GameObject pantallaGameOver;

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    public void ProcesarDerrotaOleada()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        saludPermanenteNucleo -= penalidadPorColapso;

        if (saludPermanenteNucleo <= 0f)
        {
            Debug.LogWarning("Salud permanente al 0%. Game Over Definitivo.");
            if (pantallaGameOver != null) pantallaGameOver.SetActive(true);
        }
        else
        {
            Debug.Log($"Colapso de oleada. Energía permanente restante: {saludPermanenteNucleo}%");
            if (pantallaProtocoloLazaro != null) pantallaProtocoloLazaro.SetActive(true);
        }
    }

    // --- NUEVO: VÍAS DE ESCAPE PARA LOS BOTONES DE UI ---

    // Conecta esta función al botón "Continuar / Reiniciar" del panel Protocolo Lázaro
    public void EjecutarReinicioTactico()
    {
        Time.timeScale = 1f; // Vital: si no descongelas el tiempo, el nivel recargará paralizado
        // Recarga exactamente la escena en la que estás ahora mismo
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Conecta esta función al botón "Volver al Menú" del panel Game Over
    public void RegresarAlMenuPrincipal(string nombreEscenaMenu)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaMenu);
    }
}