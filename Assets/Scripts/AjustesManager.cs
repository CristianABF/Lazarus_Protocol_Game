using UnityEngine;
using TMPro;

public class AjustesManager : MonoBehaviour
{
    [Header("Textos de la Interfaz")]
    public TextMeshProUGUI textoSonido;
    public TextMeshProUGUI textoGraficos;
    public TextMeshProUGUI textoControles;
    public TextMeshProUGUI textoGameplay;

    [Header("Valores de Sonido")]
    private int volMaestro = 8;
    private int volMusica = 5;
    private int volEfectos = 6;

    [Header("Valores de Gráficos")]
    private int resIndex = 0;
    private string[] resoluciones = { "1024x768", "1280x720", "1920x1080" };
    private int refrescoIndex = 0;
    private string[] refrescos = { "60Hz", "120Hz", "144Hz" };
    private int crtIndex = 2;
    private string[] nivelesCRT = { "OFF", "BAJO", "ALTO" };
    private int brillo = 7; // Este conserva barras

    [Header("Valores de Controles")]
    private int sensibilidad = 4; // Este conserva barras
    private bool invertirY = false;
    private bool vibracion = true;

    [Header("Valores de Gameplay")]
    private int dificultadIndex = 1;
    private string[] dificultades = { "FÁCIL", "NORMAL", "DIFÍCIL" };
    private bool ayudasVisuales = false;
    private bool subtitulos = false;

    void Start()
    {
        ActualizarPanelSonido();
        ActualizarPanelGraficos();
        ActualizarPanelControles();
        ActualizarPanelGameplay();
    }

    // --- FUNCIONES DE SONIDO (CONSERVAN MAS Y MENOS) ---
    public void ClicMaestroMas() { volMaestro = (volMaestro < 10) ? volMaestro + 1 : 0; ActualizarPanelSonido(); }
    public void ClicMaestroMenos() { volMaestro = (volMaestro > 0) ? volMaestro - 1 : 10; ActualizarPanelSonido(); }
    public void ClicMusicaMas() { volMusica = (volMusica < 10) ? volMusica + 1 : 0; ActualizarPanelSonido(); }
    public void ClicMusicaMenos() { volMusica = (volMusica > 0) ? volMusica - 1 : 10; ActualizarPanelSonido(); }
    public void ClicEfectosMas() { volEfectos = (volEfectos < 10) ? volEfectos + 1 : 0; ActualizarPanelSonido(); }
    public void ClicEfectosMenos() { volEfectos = (volEfectos > 0) ? volEfectos - 1 : 10; ActualizarPanelSonido(); }

    // --- FUNCIONES DE GRÁFICOS ---
    public void ClicResolucion() { resIndex = (resIndex < resoluciones.Length - 1) ? resIndex + 1 : 0; ActualizarPanelGraficos(); }
    public void ClicRefresco() { refrescoIndex = (refrescoIndex < refrescos.Length - 1) ? refrescoIndex + 1 : 0; ActualizarPanelGraficos(); }
    public void ClicCRT() { crtIndex = (crtIndex < nivelesCRT.Length - 1) ? crtIndex + 1 : 0; ActualizarPanelGraficos(); }

    // Brillo conserva Mas y Menos
    public void ClicBrilloMas() { brillo = (brillo < 10) ? brillo + 1 : 0; ActualizarPanelGraficos(); }
    public void ClicBrilloMenos() { brillo = (brillo > 0) ? brillo - 1 : 10; ActualizarPanelGraficos(); }

    // --- FUNCIONES DE CONTROLES ---
    // Sensibilidad conserva Mas y Menos
    public void ClicSensibilidadMas() { sensibilidad = (sensibilidad < 10) ? sensibilidad + 1 : 0; ActualizarPanelControles(); }
    public void ClicSensibilidadMenos() { sensibilidad = (sensibilidad > 0) ? sensibilidad - 1 : 10; ActualizarPanelControles(); }

    // Booleanos se invierten a sí mismos (!invierte el valor actual)
    public void ClicInvertirY() { invertirY = !invertirY; ActualizarPanelControles(); }
    public void ClicVibracion() { vibracion = !vibracion; ActualizarPanelControles(); }

    // --- FUNCIONES DE GAMEPLAY ---
    public void ClicDificultad() { dificultadIndex = (dificultadIndex < dificultades.Length - 1) ? dificultadIndex + 1 : 0; ActualizarPanelGameplay(); }
    public void ClicAyudas() { ayudasVisuales = !ayudasVisuales; ActualizarPanelGameplay(); }
    public void ClicSubtitulos() { subtitulos = !subtitulos; ActualizarPanelGameplay(); }

    // --- ACTUALIZADORES VISUALES ---
    private void ActualizarPanelSonido()
    {
        if (textoSonido == null) return;
        string barrasMusica = GenerarBarras(volMusica);
        string barrasEfectos = GenerarBarras(volEfectos);
        textoSonido.text =
            "─── SONIDO ───\n" +
            $"VOLUMEN MAESTRO: [{volMaestro}/10]\n" +
            $"MÚSICA:  [{volMusica}/10] {barrasMusica}\n" +
            $"EFECTOS: [{volEfectos}/10] {barrasEfectos}";
    }

    private void ActualizarPanelGraficos()
    {
        if (textoGraficos == null) return;

        string barrasBrillo = GenerarBarras(brillo);

        textoGraficos.text =
            "─── GRÁFICOS ───\n" +
            $"RESOLUCIÓN: [{resoluciones[resIndex]}]\n" +
            $"REFRESCO:   [{refrescos[refrescoIndex]}]\n" +
            $"EFECTO CRT: [{nivelesCRT[crtIndex]}]\n" +
            $"BRILLO:     [{brillo,2}/10]\n" +
            $"{barrasBrillo}";
    }

    private void ActualizarPanelControles()
    {
        if (textoControles == null) return;
        string strInvertir = invertirY ? "ON" : "OFF";
        string strVibracion = vibracion ? "ON" : "OFF";
        textoControles.text =
            "─── CONTROLES ───\n" +
            $"SENSIBILIDAD MOUSE: [{sensibilidad,2}/10]\n" +
            $"INVERTIR Y:         [{strInvertir}]\n" +
            $"VIBRACIÓN:          [{strVibracion}]\n\n" +
            ">> REASIGNAR TECLAS";
    }

    private void ActualizarPanelGameplay()
    {
        if (textoGameplay == null) return;
        string strAyudas = ayudasVisuales ? "ON" : "OFF";
        string strSubtitulos = subtitulos ? "ON" : "OFF";
        textoGameplay.text =
            "─── GAMEPLAY ───\n" +
            $"DIFICULTAD:      [{dificultades[dificultadIndex]}]\n" +
            $"AYUDAS VISUALES: [{strAyudas}]\n" +
            $"SUBTÍTULOS:      [{strSubtitulos}]";
    }

    private string GenerarBarras(int valor)
    {
        string barras = "";
        for (int i = 0; i < 10; i++)
        {
            if (i < valor) barras += "█";
            else barras += "▒";
        }
        return barras;
    }
}