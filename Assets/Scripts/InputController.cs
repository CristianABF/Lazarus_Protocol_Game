using UnityEngine;

public class InputController : MonoBehaviour
{
    public static InputManager Input { get; private set; }

    private void Awake()
    {
        if (Input == null)
        {
            Input = new InputManager();
            Input.Player.Enable();
        }
        else { Destroy(gameObject);}
    }

    private void OnDestroy()
    {
        if (Input != null )
        {
            Input.Player.Disable();
            Input.Disable();
            Input.Dispose();
            Input = null;
        }
    }
}
