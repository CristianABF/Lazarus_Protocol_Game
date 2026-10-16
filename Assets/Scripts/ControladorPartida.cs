using UnityEngine;
using TMPro; // Obligatorio para modificar textos de la UI
using UnityEngine.SceneManagement;
using System.Collections.Generic; // NUEVO: Obligatorio para usar listas (List<>)

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
    public GameObject panelMejoras;
    public TextMeshProUGUI textoEstadisticasGameOver;

    [Header("Meta-Economía (Persistente)")]
    public int creditosActuales = 0;
    public float multiplicadorCreditos = 2.5f; // Balance: 10 min (600s) * 2.5 = 1500 PTS

    [Header("Inventario de Estadísticas (Memoria Inmortal)")]

    // NUEVO: Registro visual para que los botones de la UI recuerden si fueron comprados
    public List<string> mejorasCompradas = new List<string>();

    // --- SALUD ---
    public float saludExtraJugador = 0f;

    // --- VELOCIDAD ---
    public float velocidadCaminarExtra = 0f; // Porcentaje a sumar (ej. 0.15f)
    public float velMaximaCaminarExtra = 0f; // Porcentaje a sumar (ej. 0.35f)
    public float factorRecargaEstaciones = 1f; // 1f es velocidad normal. 0.7f es 30% más rápido.

    // --- DAÑO ---
    public int dañoExtraArma = 0;
    public int balasExtraCargador = 0;

    // --- DEFENSA ---
    public float factorRecargaTorretas = 1f; // 1f es normal. 0.6f es 40% más rápido.
    public int cargadoresExtraMaximos = 0;
    public float factorDañoTorretas = 1f; // 1f es normal. 1.2f es 20% extra.

    public void RegistrarMejoraAdquirida(string nombreMejora)
    {
        // 1. Guardar el nombre exacto de la mejora para que los nuevos botones UI lo recuerden al recargar la escena
        if (!mejorasCompradas.Contains(nombreMejora))
        {
            mejorasCompradas.Add(nombreMejora);
        }

        // 2. Inyectar la matemática de combate
        switch (nombreMejora)
        {
            // --- RAMA SALUD ---
            case "SALUD I": saludExtraJugador += 25f; break;
            case "SALUD II": saludExtraJugador += 50f; break;
            case "SALUD III": saludExtraJugador += 100f; break;

            // --- RAMA VELOCIDAD ---
            case "VELOCIDAD I": velocidadCaminarExtra += 0.15f; break;
            case "VELOCIDAD II": factorRecargaEstaciones -= 0.30f; break;
            case "VELOCIDAD III": velMaximaCaminarExtra += 0.35f; break;

            // --- RAMA DAÑO ---
            case "DAÑO I": dañoExtraArma += 3; break;
            case "DAÑO II": balasExtraCargador += 15; break; // Modificará maxAmmo
            case "DAÑO III": dañoExtraArma += 15; break;

            // --- RAMA DEFENSA ---
            case "DEFENSA I": factorRecargaTorretas -= 0.40f; break;
            case "DEFENSA II": cargadoresExtraMaximos += 5; break; // Modificará maxClips
            case "DEFENSA III": factorDañoTorretas += 0.20f; break;
        }

        Debug.Log($"Mejora procesada: {nombreMejora}. Bóveda actualizada.");
    }

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
        // 1. Congela la acción y libera el ratón
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. LIQUIDACIÓN ECONÓMICA (NUEVO)
        // Calculamos cuánto tiempo real pasó desde el Start() hasta esta muerte
        float tiempoSobrevivido = Time.time - tiempoInicioOleada;

        // Ejecutamos el pago inyectando los segundos en tu función
        LiquidarTiempoDeAsedio(tiempoSobrevivido);

        // 3. Consecuencias del colapso
        saludPermanenteNucleo -= penalidadPorColapso;

        if (saludPermanenteNucleo <= 0f)
        {
            Debug.LogWarning("Salud permanente al 0%. Game Over Definitivo.");
            ActualizarTextoEstadisticas();
            if (pantallaGameOver != null) pantallaGameOver.SetActive(true);
        }
        else
        {
            Debug.Log($"Colapso de oleada. Energía permanente restante: {saludPermanenteNucleo}%");
            if (pantallaProtocoloLazaro != null) pantallaProtocoloLazaro.SetActive(true);
        }
    }

    public void LiquidarTiempoDeAsedio(float segundosSobrevividos)
    {
        int ganancia = Mathf.RoundToInt(segundosSobrevividos * multiplicadorCreditos);
        creditosActuales += ganancia;
        Debug.Log($"Asedio colapsado. Tiempo: {segundosSobrevividos}s. Ganancia: {ganancia} PTS. Saldo Total: {creditosActuales} PTS");
    }

    // --- VÍAS DE ESCAPE ---
    public void EjecutarReinicioTactico()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AbrirTiendaMejoras()
    {
        if (pantallaProtocoloLazaro != null) pantallaProtocoloLazaro.SetActive(false);
        if (panelMejoras != null) panelMejoras.SetActive(true);
        Debug.Log("Transición a la Tienda de Mejoras completada.");
    }

    public void RendirseYMostrarGameOver()
    {
        if (pantallaProtocoloLazaro != null) pantallaProtocoloLazaro.SetActive(false);
        ActualizarTextoEstadisticas();
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

        float tiempoSobrevivido = Time.time - tiempoInicioOleada;
        int minutos = Mathf.FloorToInt(tiempoSobrevivido / 60f);
        int segundos = Mathf.FloorToInt(tiempoSobrevivido % 60f);
        string tiempoFormateado = string.Format("{0:00}:{1:00} MIN", minutos, segundos);

        textoEstadisticasGameOver.text =
            "       --- ESTADÍSTICAS FINALES ---\n\n" +
            $"Tiempo total:             {tiempoFormateado}\n" +
            $"Salud restante:           0%\n" +
            $"Enemigos derrotados:      {enemigosDerrotados}\n" +
            $"Daño recibido:            {danoRecibidoTotal}";
    }
}