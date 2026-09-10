using UnityEngine;
using UnityEngine.SceneManagement;



public class SwapScene : MonoBehaviour
{
    public void CargarEscena(string nombreDeLaEscena)
    {
        SceneManager.LoadScene(nombreDeLaEscena);
    }
    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
