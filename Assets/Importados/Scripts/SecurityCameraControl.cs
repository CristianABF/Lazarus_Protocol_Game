using UnityEngine;

public class SecurityCameraControl : MonoBehaviour
{
    [Header("Giro y Patrullaje")]
    public Transform cameraHead;
    public float rotationAngle = 45f;
    public float rotationSpeed = 2f;

    [Header("POV / Vista de la Cámara")]
    public Camera povCamera; // El componente Camera nativo pegado al cabezal

    private Quaternion startRotation;

    void Start()
    {
        if (cameraHead != null)
        {
            startRotation = cameraHead.localRotation;
        }

        // Por defecto mantenemos el POV apagado hasta que el jugador ingrese
        if (povCamera != null)
        {
            povCamera.enabled = false;
        }
    }

    void Update()
    {
        Patrol();
    }

    void Patrol()
    {
        if (cameraHead == null) return;

        float angle = Mathf.Sin(Time.time * rotationSpeed) * rotationAngle;
        cameraHead.localRotation = startRotation * Quaternion.Euler(0, angle, 0);
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