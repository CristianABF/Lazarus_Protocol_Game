using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AjustesManager : MonoBehaviour
{
    [Header("Textos de la Interfaz")]
    public TextMeshProUGUI textoSonido;
    public TextMeshProUGUI textoGraficos;
    public TextMeshProUGUI textoControles;
    public TextMeshProUGUI textoGameplay;

    [Header("Motor de Audio")]
    public AudioMixer audioMixer;

    [Header("Hardware Visual")]
    public Volume volumenPostProcesado;
    private ColorAdjustments controlColor;

    [Header("Valores Internos de Sonido")]
    private int volMaestro = 8;
    private int volMusica = 5;
    private int volEfectos = 6;

    [Header("Valores de Gráficos")]
    private int resIndex = 0;
    private string[] resoluciones = { "1024x768", "1280x720", "1920x1080" };
    private int refrescoIndex = 0;
    private string[] refrescos = { "60Hz", "120Hz", "144Hz" };
    private int brillo = 7;

    [Header("Valores de Controles")]
    private int sensibilidad = 4;
    private bool invertirY = false;

    [Header("Valores de Gameplay")]
    private int dificultadIndex = 1;
    private string[] dificultades = { "FÁCIL", "NORMAL", "DIFÍCIL" };
    private bool ayudasVisuales = false;
    private bool subtitulos = false;

    void Start()
    {
        // 1. LECTURA DEL DISCO DURO
        volMaestro = PlayerPrefs.GetInt("Pref_VolMaestro", 8);
        volMusica = PlayerPrefs.GetInt("Pref_VolMusica", 5);
        volEfectos = PlayerPrefs.GetInt("Pref_VolEfectos", 6);

        resIndex = PlayerPrefs.GetInt("Pref_ResIndex", 0);
        refrescoIndex = PlayerPrefs.GetInt("Pref_Refresco", 0);
        brillo = PlayerPrefs.GetInt("Pref_Brillo", 7);

        sensibilidad = PlayerPrefs.GetInt("Pref_Sensibilidad", 4);
        invertirY = PlayerPrefs.GetInt("Pref_InvertirY", 0) == 1;

        // 2. CONEXIÓN FÍSICA INICIAL 
        AudioListener.volume = volMaestro / 10f;
        AplicarVolumenMixer("VolMusicaParam", volMusica);
        AplicarVolumenMixer("VolEfectosParam", volEfectos);

        EjecutarResolucion();
        EjecutarFPS();

        if (volumenPostProcesado != null)
        {
            volumenPostProcesado.profile.TryGet(out controlColor);
        }
        EjecutarBrilloFisico();

        // 3. DIBUJAR PANELES
        ActualizarPanelSonido();
        ActualizarPanelGraficos();
        ActualizarPanelControles();
        ActualizarPanelGameplay();
    }

    // ==========================================
    // --- FUNCIONES DE SONIDO ---
    // ==========================================
    public void ClicMaestroMas() { volMaestro = (volMaestro < 10) ? volMaestro + 1 : 0; GuardarMaestro(); }
    public void ClicMaestroMenos() { volMaestro = (volMaestro > 0) ? volMaestro - 1 : 10; GuardarMaestro(); }
    public void ClicMusicaMas() { volMusica = (volMusica < 10) ? volMusica + 1 : 0; GuardarMusica(); }
    public void ClicMusicaMenos() { volMusica = (volMusica > 0) ? volMusica - 1 : 10; GuardarMusica(); }
    public void ClicEfectosMas() { volEfectos = (volEfectos < 10) ? volEfectos + 1 : 0; GuardarEfectos(); }
    public void ClicEfectosMenos() { volEfectos = (volEfectos > 0) ? volEfectos - 1 : 10; GuardarEfectos(); }

    private void GuardarMaestro()
    {
        PlayerPrefs.SetInt("Pref_VolMaestro", volMaestro);
        AudioListener.volume = volMaestro / 10f;
        ActualizarPanelSonido();
    }

    private void GuardarMusica()
    {
        PlayerPrefs.SetInt("Pref_VolMusica", volMusica);
        AplicarVolumenMixer("VolMusicaParam", volMusica);
        ActualizarPanelSonido();
    }

    private void GuardarEfectos()
    {
        PlayerPrefs.SetInt("Pref_VolEfectos", volEfectos);
        AplicarVolumenMixer("VolEfectosParam", volEfectos);
        ActualizarPanelSonido();
    }

    private void AplicarVolumenMixer(string parametro, int valor0a10)
    {
        if (audioMixer == null) return;
        if (valor0a10 == 0) audioMixer.SetFloat(parametro, -80f);
        else audioMixer.SetFloat(parametro, Mathf.Log10(valor0a10 / 10f) * 20f);
    }

    // ==========================================
    // --- FUNCIONES DE GRÁFICOS ---
    // ==========================================
    public void ClicResolucion()
    {
        resIndex = (resIndex < resoluciones.Length - 1) ? resIndex + 1 : 0;
        PlayerPrefs.SetInt("Pref_ResIndex", resIndex);
        EjecutarResolucion();
        ActualizarPanelGraficos();
    }

    public void ClicRefresco()
    {
        refrescoIndex = (refrescoIndex < refrescos.Length - 1) ? refrescoIndex + 1 : 0;
        PlayerPrefs.SetInt("Pref_Refresco", refrescoIndex);
        EjecutarFPS();
        ActualizarPanelGraficos();
    }

    public void ClicBrilloMas() { brillo = (brillo < 10) ? brillo + 1 : 0; GuardarBrillo(); }
    public void ClicBrilloMenos() { brillo = (brillo > 0) ? brillo - 1 : 10; GuardarBrillo(); }

    private void GuardarBrillo()
    {
        PlayerPrefs.SetInt("Pref_Brillo", brillo);
        EjecutarBrilloFisico();
        ActualizarPanelGraficos();
    }

    private void EjecutarResolucion()
    {
        string[] dim = resoluciones[resIndex].Split('x');
        Screen.SetResolution(int.Parse(dim[0]), int.Parse(dim[1]), true);
    }

    private void EjecutarFPS()
    {
        Application.targetFrameRate = int.Parse(refrescos[refrescoIndex].Replace("Hz", ""));
    }

    private void EjecutarBrilloFisico()
    {
        if (controlColor != null)
        {
            controlColor.postExposure.value = (brillo - 5f) * 0.5f;
        }
    }

    // ==========================================
    // --- FUNCIONES DE CONTROLES ---
    // ==========================================
    public void ClicSensibilidadMas()
    {
        sensibilidad = (sensibilidad < 10) ? sensibilidad + 1 : 0;
        PlayerPrefs.SetInt("Pref_Sensibilidad", sensibilidad);
        ActualizarPanelControles();
    }
    public void ClicSensibilidadMenos()
    {
        sensibilidad = (sensibilidad > 0) ? sensibilidad - 1 : 10;
        PlayerPrefs.SetInt("Pref_Sensibilidad", sensibilidad);
        ActualizarPanelControles();
    }

    public void ClicInvertirY()
    {
        invertirY = !invertirY;
        PlayerPrefs.SetInt("Pref_InvertirY", invertirY ? 1 : 0);
        ActualizarPanelControles();
    }

    // ==========================================
    // --- FUNCIONES DE GAMEPLAY ---
    // ==========================================
    public void ClicDificultad() { dificultadIndex = (dificultadIndex < dificultades.Length - 1) ? dificultadIndex + 1 : 0; ActualizarPanelGameplay(); }
    public void ClicAyudas() { ayudasVisuales = !ayudasVisuales; ActualizarPanelGameplay(); }
    public void ClicSubtitulos() { subtitulos = !subtitulos; ActualizarPanelGameplay(); }

    // ==========================================
    // --- ACTUALIZADORES VISUALES FIJOS ---
    // ==========================================
    private void ActualizarPanelSonido()
    {
        if (textoSonido == null) return;
        string barrasMusica = GenerarBarras(volMusica);
        string barrasEfectos = GenerarBarras(volEfectos);

        textoSonido.text =
            "      ─── SONIDO ───\n" +
            $"VOLUMEN MAESTRO: [{volMaestro,2}/10]\n" +
            $"MÚSICA:  [{volMusica,2}/10] {barrasMusica}\n" +
            $"EFECTOS: [{volEfectos,2}/10] {barrasEfectos}";
    }

    private void ActualizarPanelGraficos()
    {
        if (textoGraficos == null) return;
        string barrasBrillo = GenerarBarras(brillo);

        textoGraficos.text =
            "      ─── GRÁFICOS ───\n" +
            $"RESOLUCIÓN: [{resoluciones[resIndex]}]\n" +
            $"REFRESCO:   [{refrescos[refrescoIndex]}]\n" +
            $"BRILLO:     [{brillo,2}/10] {barrasBrillo}";
    }

    private void ActualizarPanelControles()
    {
        if (textoControles == null) return;
        string strInvertir = invertirY ? "ON" : "OFF";

        textoControles.text =
            "      ─── CONTROLES ───\n" +
            $"SENSIBILIDAD MOUSE: [{sensibilidad,2}/10]\n" +
            $"INVERTIR Y:         [{strInvertir}]";
    }

    private void ActualizarPanelGameplay()
    {
        if (textoGameplay == null) return;
        string strAyudas = ayudasVisuales ? "ON" : "OFF";
        string strSubtitulos = subtitulos ? "ON" : "OFF";

        textoGameplay.text =
            "      ─── GAMEPLAY ───\n" +
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