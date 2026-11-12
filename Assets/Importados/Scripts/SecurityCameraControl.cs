using UnityEngine;

public class SecurityCameraControl : MonoBehaviour
{
    [Header("POV / Vista de la Cámara")]
    public Camera povCamera; // El componente Camera nativo pegado al cabezal

    void Start()
    {
        // Por defecto mantenemos el POV apagado hasta que el jugador ingrese
        if (povCamera != null)
        {
            povCamera.enabled = false;
        }
    }

    // Método para que el jugador pase a ver en primera persona a través de esta cámara
    public void ActivarPOV(bool activar)
    {
        if (povCamera != null)
        {
            povCamera.enabled = activar;
        }
    }
}