using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseControl : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public static bool isPaused = false;
    
    // Update is called once per frame
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
        Time.timeScale = 1f; // el tiempo vuelve a la normalidad
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true); // muestra el canvas
        Time.timeScale = 0f; // congela el tiempo
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
