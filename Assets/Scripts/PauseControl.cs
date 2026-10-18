using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseControl : MonoBehaviour
{
    [Header("Paneles de UI")]
    public GameObject pauseMenuUI; // El Canvas principal de la pausa
    public GameObject pauseBlurVolume;

    [Header("Sub-Menús (Arrastrar desde la jerarquía)")]
    public GameObject listaBotones;
    public GameObject panelGuardar;
    public GameObject panelAjustes;

    [Header("HUD")]
    public PlayerHealthBar hudSalud; // Arrastrar la barra de salud desde la jerarquía

    public static bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // oculta el canvas
        if (pauseBlurVolume != null) pauseBlurVolume.SetActive(false);

        // Asegurar que los sub-menús se apaguen
        if (listaBotones != null) listaBotones.SetActive(false);
        if (panelGuardar != null) panelGuardar.SetActive(false);
        if (panelAjustes != null) panelAjustes.SetActive(false);

        // MOSTRAR LA VIDA AL REANUDAR EL JUEGO
        if (hudSalud != null)
        {
            hudSalud.SetHUDVisible(true);
        }

        Time.timeScale = 1f; // el tiempo vuelve a la normalidad
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // Función interna para Pausar
    void Pause()
    {
        pauseMenuUI.SetActive(true); // muestra el canvas
        if (pauseBlurVolume != null) pauseBlurVolume.SetActive(true);

        // Mostrar la lista de botones y ocultar los otros paneles
        if (listaBotones != null) listaBotones.SetActive(true);
        if (panelGuardar != null) panelGuardar.SetActive(false);
        if (panelAjustes != null) panelAjustes.SetActive(false);

        // OCULTAR LA VIDA AL PONER PAUSA
        if (hudSalud != null)
        {
            hudSalud.SetHUDVisible(false);
        }

        Time.timeScale = 0f; // congela el tiempo
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
        Debug.Log("Saliendo de Lazarus Protocol...");
        Application.Quit();
    }
}