using UnityEngine;
using UnityEngine.SceneManagement;



public class SwapScene : MonoBehaviour
{
    public void CargarEscena(string nombreDeLaEscena)
    {
        Time.timeScale = 1f;
        PauseControl.isPaused = false;

        SceneManager.LoadScene(nombreDeLaEscena);
    }
    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
