using UnityEngine;

public class MainMenuInit : MonoBehaviour
{
    [Header("Navegación del Menú")]
    public GameObject listaBotonesPrincipal;
    public GameObject panelAjustes;
    public GameObject panelCargar;

    void Start()
    {
        Time.timeScale = 1f;
        PauseControl.isPaused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Tecla de retroceso (ESC)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (panelAjustes != null && panelAjustes.activeSelf)
            {
                panelAjustes.SetActive(false);
                if (listaBotonesPrincipal != null) listaBotonesPrincipal.SetActive(true);
            }
            else if (panelCargar != null && panelCargar.activeSelf)
            {
                panelCargar.SetActive(false);
                if (listaBotonesPrincipal != null) listaBotonesPrincipal.SetActive(true);
            }
        }
    }
}