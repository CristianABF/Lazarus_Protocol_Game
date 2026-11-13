using UnityEngine;

public class MainMenuInit : MonoBehaviour
{
    [Header("Navegación del Menú")]
    public GameObject listaBotonesPrincipal;
    public GameObject panelAjustes;
    public GameObject panelCargar;

    void Start()
    {
        // 1. Configuraciones base de inicio
        Time.timeScale = 1f;
        PauseControl.isPaused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // 2. Escuchar la tecla de retroceso (ESC)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Si el Panel de Ajustes está encendido, lo apaga y vuelve al menú
            if (panelAjustes != null && panelAjustes.activeSelf)
            {
                panelAjustes.SetActive(false);
                if (listaBotonesPrincipal != null) listaBotonesPrincipal.SetActive(true);
            }
            // Si el Panel de Cargar está encendido, hace lo mismo
            else if (panelCargar != null && panelCargar.activeSelf)
            {
                panelCargar.SetActive(false);
                if (listaBotonesPrincipal != null) listaBotonesPrincipal.SetActive(true);
            }
        }
    }
}