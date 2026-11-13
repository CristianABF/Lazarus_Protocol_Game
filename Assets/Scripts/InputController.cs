using UnityEngine;

public class InputController : MonoBehaviour
{
    public static InputManager Input { get; private set; }

    private static InputController Instancia;

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            Input = new InputManager();
            Input.Player.Enable();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instancia == this && Input != null)
        {
            Input.Player.Disable();
            Input.Disable();
            Input.Dispose();
            Input = null;
        }
    }
}