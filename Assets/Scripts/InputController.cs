using UnityEngine;

public class InputController : MonoBehaviour
{
    public static InputManager Input { get; private set; }

    // NUEVO: Identificador físico del objeto original
    private static InputController Instancia;

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            Input = new InputManager();
            Input.Player.Enable();
            DontDestroyOnLoad(gameObject); // Inmortalidad
        }
        else
        {
            Destroy(gameObject); // El clon se suicida
        }
    }

    private void OnDestroy()
    {
        // NUEVA REGLA: Solo el dueño original tiene derecho a formatear la memoria
        if (Instancia == this && Input != null)
        {
            Input.Player.Disable();
            Input.Disable();
            Input.Dispose();
            Input = null;
        }
    }
}