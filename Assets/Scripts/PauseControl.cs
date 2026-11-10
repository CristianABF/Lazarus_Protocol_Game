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

    [Header("Referencias del Jugador y HUD")]
    public GameObject jugador; // Variable nueva para saber a quién guardar
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

    public void AbrirGuardar() { listaBotones.SetActive(false); panelGuardar.SetActive(true); }
    public void AbrirAjustes() { listaBotones.SetActive(false); panelAjustes.SetActive(true); }
    public void VolverMenuPausa() { panelGuardar.SetActive(false); panelAjustes.SetActive(false); listaBotones.SetActive(true); }

    // --- NUEVO: SISTEMA DE GUARDADO ---
    public void GuardarPartida()
    {
        if (jugador != null)
        {
            PlayerPrefs.SetFloat("PlayerX", jugador.transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", jugador.transform.position.y);
            PlayerPrefs.SetFloat("PlayerZ", jugador.transform.position.z);
            PlayerPrefs.Save();
            Debug.Log("¡Partida Guardada!");
        }
    }

    public void CargarPartida()
    {
        if (PlayerPrefs.HasKey("PlayerX") && jugador != null)
        {
            float x = PlayerPrefs.GetFloat("PlayerX");
            float y = PlayerPrefs.GetFloat("PlayerY");
            float z = PlayerPrefs.GetFloat("PlayerZ");

            CharacterController controller = jugador.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            jugador.transform.position = new Vector3(x, y, z);

            if (controller != null) controller.enabled = true;

            Debug.Log("¡Partida Cargada!");
            Resume(); // Reanuda el juego automáticamente
        }
    }
    // ---------------------------------

    public void IrMenuPrincipal(string nombreEscenaMenu)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    public void SalirJuego() { Application.Quit(); }
}