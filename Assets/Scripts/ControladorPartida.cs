using UnityEngine;
using TMPro; // Obligatorio para modificar textos de la UI
using UnityEngine.SceneManagement;

public class ControladorPartida : MonoBehaviour
{
    public static ControladorPartida Instancia;

    [Header("Meta-Progresión de la Nave")]
    public float saludPermanenteNucleo = 100f;
    public float penalidadPorColapso = 15f;

    [Header("Estadísticas de la Oleada Actual (En Vivo)")]
    public int enemigosDerrotados = 0;
    public float danoRecibidoTotal = 0f;
    private float tiempoInicioOleada;

    [Header("Pantallas de Fin de Partida")]
    public GameObject pantallaProtocoloLazaro;
    public GameObject pantallaGameOver;
    public TextMeshProUGUI textoEstadisticasGameOver; // NUEVO: El puente lógico hacia la UI

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject); // El escudo contra el borrado de escena
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Iniciamos el cronómetro en el instante en que carga el nivel
        tiempoInicioOleada = Time.time;
    }

    // --- NUEVOS RECEPTORES DE DATOS ---
    public void RegistrarEnemigoDerrotado()
    {
        enemigosDerrotados++;
    }

    public void RegistrarDanoRecibido(float cantidad)
    {
        danoRecibidoTotal += cantidad;
    }

    // ----------------------------------

    public void ProcesarDerrotaOleada()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        saludPermanenteNucleo -= penalidadPorColapso;

        if (saludPermanenteNucleo <= 0f)
        {
            Debug.LogWarning("Salud permanente al 0%. Game Over Definitivo.");
            ActualizarTextoEstadisticas(); // Inyecta la matemática antes de mostrar el panel
            if (pantallaGameOver != null) pantallaGameOver.SetActive(true);
        }
        else
        {
            Debug.Log($"Colapso de oleada. Energía permanente restante: {saludPermanenteNucleo}%");
            if (pantallaProtocoloLazaro != null) pantallaProtocoloLazaro.SetActive(true);
        }
    }

    // --- VÍAS DE ESCAPE ---

    public void EjecutarReinicioTactico()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RendirseYMostrarGameOver()
    {
        if (pantallaProtocoloLazaro != null) pantallaProtocoloLazaro.SetActive(false);

        ActualizarTextoEstadisticas(); // Inyecta la matemática antes de mostrar el panel

        if (pantallaGameOver != null) pantallaGameOver.SetActive(true);
    }

    public void RegresarAlMenuPrincipal(string nombreEscenaMenu)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    // --- EL MOTOR DE DIBUJADO DE ESTADÍSTICAS ---
    private void ActualizarTextoEstadisticas()
    {
        if (textoEstadisticasGameOver == null) return;

        // Calcula el tiempo real transcurrido
        float tiempoSobrevivido = Time.time - tiempoInicioOleada;
        int minutos = Mathf.FloorToInt(tiempoSobrevivido / 60f);
        int segundos = Mathf.FloorToInt(tiempoSobrevivido % 60f);
        string tiempoFormateado = string.Format("{0:00}:{1:00} MIN", minutos, segundos);

        // Reescribe el texto estático con las variables dinámicas
        textoEstadisticasGameOver.text =
            "       --- ESTADÍSTICAS FINALES ---\n\n" +
            $"Tiempo total:             {tiempoFormateado}\n" +
            $"Salud restante:           0%\n" +
            $"Enemigos derrotados:      {enemigosDerrotados}\n" +
            $"Daño recibido:            {danoRecibidoTotal}";
    }
}