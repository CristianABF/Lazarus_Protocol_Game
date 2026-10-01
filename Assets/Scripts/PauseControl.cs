using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseControl : MonoBehaviour
{
    [Header("Paneles de UI")]
    public GameObject pauseMenuUI; // El Canvas principal de la pausa
    public GameObject pauseBlurVolume;

    [Header("Sub-Menús (Arrastrar desde la jerarquía)")]
    public GameObject listaBotones; // Arrastrar el objeto "ListaBotones"
    public GameObject panelGuardar; // Arrastrar "Panel_Guardar"
    public GameObject panelAjustes; // Arrastrar "Panel_Ajustes"

    public static bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Al presionar ESC, si está pausado cierra todo, sino pausa.
            if (isPaused) Resume();
            else Pause();
        }
    }

    // 1. CONTINUAR
    public void Resume()
    {
        pauseMenuUI.SetActive(false); // oculta el canvas
        if (pauseBlurVolume != null) pauseBlurVolume.SetActive(false);

        // Asegurar que los sub-menús se apaguen
        if (listaBotones != null) listaBotones.SetActive(false);
        if (panelGuardar != null) panelGuardar.SetActive(false);
        if (panelAjustes != null) panelAjustes.SetActive(false);

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

        Time.timeScale = 0f; // congela el tiempo
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 2. GUARDAR PARTIDA
    public void AbrirGuardar()
    {
        listaBotones.SetActive(false);
        panelGuardar.SetActive(true);
    }

    // 3. AJUSTES
    public void AbrirAjustes()
    {
        listaBotones.SetActive(false);
        panelAjustes.SetActive(true);
    }

    // BOTÓN VOLVER (Para usarlo dentro de Guardar o Ajustes)
    public void VolverMenuPausa()
    {
        panelGuardar.SetActive(false);
        panelAjustes.SetActive(false);
        listaBotones.SetActive(true);
    }

    // 4. MENÚ PRINCIPAL
    public void IrMenuPrincipal(string nombreEscenaMenu)
    {
        Time.timeScale = 1f; // OBLIGATORIO: Si no devuelves el tiempo a 1, el menú principal cargará congelado
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    // 5. SALIR
    public void SalirJuego()
    {
        Debug.Log("Saliendo de Lazarus Protocol...");
        Application.Quit();
    }
}