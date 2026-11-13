using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseControl : MonoBehaviour
{
    [Header("Paneles de UI")]
    public GameObject pauseMenuUI;
    public GameObject pauseBlurVolume;

    [Header("Sub-Menús")]
    public GameObject listaBotones;
    public GameObject panelGuardar;
    public GameObject panelAjustes;

    [Header("Referencias del HUD")]
    public PlayerHealthBar hudSalud;

    public static bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    [Header("Conexión Interna")]
    public AjustesManager scriptAjustes;

    void Start()
    {
        // Carga el sonido y gráficos
        if (scriptAjustes != null)
        {
            scriptAjustes.CargarAjustes();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        if (pauseBlurVolume != null) pauseBlurVolume.SetActive(false);
        if (listaBotones != null) listaBotones.SetActive(false);
        if (panelGuardar != null) panelGuardar.SetActive(false);
        if (panelAjustes != null) panelAjustes.SetActive(false);
        if (hudSalud != null) hudSalud.SetHUDVisible(true);

        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        if (pauseBlurVolume != null) pauseBlurVolume.SetActive(true);
        if (listaBotones != null) listaBotones.SetActive(true);
        if (panelGuardar != null) panelGuardar.SetActive(false);
        if (panelAjustes != null) panelAjustes.SetActive(false);
        if (hudSalud != null) hudSalud.SetHUDVisible(false);

        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void AbrirGuardar()
    {
        listaBotones.SetActive(false);
        panelGuardar.SetActive(true);
    }

    public void AbrirAjustes()
    {
        listaBotones.SetActive(false);
        panelAjustes.SetActive(true);
    }

    public void VolverMenuPausa()
    {
        panelGuardar.SetActive(false);
        panelAjustes.SetActive(false);
        listaBotones.SetActive(true);
    }

    public void IrMenuPrincipal(string nombreEscenaMenu)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    public void SalirJuego()
    {
        Application.Quit();
    }
}