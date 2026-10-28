using UnityEngine;

public class SecurityCameraControl : MonoBehaviour
{
    [Header("Giro y Patrullaje")]
    public Transform cameraHead;
    public float rotationAngle = 45f;
    public float rotationSpeed = 2f;

    [Header("Detección del Jugador")]
    public Transform player;
    public float detectionRadius = 10f;
    public float detectionAngle = 45f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Indicadores Visuales")]
    public Light spotLight;
    public Color normalColor = Color.green;
    public Color alertColor = Color.red;

    private Quaternion startRotation;
    private bool playerDetected = false;

    void Start()
    {
        if (cameraHead != null)
        {
            startRotation = cameraHead.localRotation;
        }

        if (spotLight != null)
        {
            spotLight.color = normalColor;
        }
    }

    void Update()
    {
        DetectPlayer();

        if (!playerDetected)
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (cameraHead == null) return;

        float angle = Mathf.Sin(Time.time * rotationSpeed) * rotationAngle;
        cameraHead.localRotation = startRotation * Quaternion.Euler(0, angle, 0);
    }

    void DetectPlayer()
    {
        if (player == null) return;

        Vector3 dirToPlayer = (player.position - cameraHead.position).normalized;
        float distanceToPlayer = Vector3.Distance(cameraHead.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {
            if (Vector3.Angle(cameraHead.forward, dirToPlayer) < detectionAngle / 2f)
            {
                if (!Physics.Raycast(cameraHead.position, dirToPlayer, distanceToPlayer, obstacleLayer))
                {
                    OnPlayerSpotted();
                    return;
                }
            }
        }

        if (playerDetected)
        {
            OnPlayerLost();
        }
    }

    void OnPlayerSpotted()
    {
        if (!playerDetected)
        {
            playerDetected = true;
            Debug.Log("¡Cámara: JUGADOR DETECTADO!");

            if (spotLight != null)
            {
                spotLight.color = alertColor;
            }
        }
    }

    void OnPlayerLost()
    {
        playerDetected = false;
        Debug.Log("Cámara: Jugador fuera de vista.");

        if (spotLight != null)
        {
            spotLight.color = normalColor;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (cameraHead == null) return;

        Gizmos.color = playerDetected ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(cameraHead.position, detectionRadius);

        Vector3 fwdLeft = Quaternion.Euler(0, -detectionAngle / 2, 0) * cameraHead.forward;
        Vector3 fwdRight = Quaternion.Euler(0, detectionAngle / 2, 0) * cameraHead.forward;

        Gizmos.DrawRay(cameraHead.position, fwdLeft * detectionRadius);
        Gizmos.DrawRay(cameraHead.position, fwdRight * detectionRadius);
    }
}