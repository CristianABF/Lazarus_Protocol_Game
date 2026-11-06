using UnityEngine;

public class TabletCameraManager : MonoBehaviour
{
    [Header("Configuración de Transmisión")]
    public RenderTexture cameraFeedRT;
    public SecurityCameraControl[] camaras;

    [Header("Interacción y Controles")]
    public KeyCode teclaInteractuar = KeyCode.C;
    public KeyCode teclaCamAnterior = KeyCode.A;
    public KeyCode teclaCamSiguiente = KeyCode.D;
    public Transform manoJugador;
    public float distanciaInteraccion = 2f;

    [Header("Posición en la mano (valores locales)")]
    public Vector3 posicionEnMano = new Vector3(0f, -0.15f, 0.45f);
    public Vector3 rotacionEnMano = new Vector3(0f, 0f, 0f);

    private int indiceCamaraActual = 0;
    private bool estaEquipada = false;
    private Vector3 posicionOriginal;
    private Quaternion rotacionOriginal;
    private Transform padreOriginal;

    void Start()
    {
        posicionOriginal = transform.localPosition;
        rotacionOriginal = transform.localRotation;
        padreOriginal = transform.parent;

        ActivarCamaraActual();
    }

    void Update()
    {
        // Equipar / soltar con la tecla de interacción
        if (Input.GetKeyDown(teclaInteractuar))
        {
            if (estaEquipada)
            {
                SoltarTablet();
            }
            else
            {
                float distancia = Vector3.Distance(transform.position, manoJugador.position);
                if (distancia <= distanciaInteraccion)
                {
                    EquiparTablet();
                }
            }
        }

        // Cambiar de cámara solo con la tablet en la mano
        if (estaEquipada && camaras.Length > 0)
        {
            if (Input.GetKeyDown(teclaCamSiguiente))
            {
                CambiarCamara(1);
            }
            else if (Input.GetKeyDown(teclaCamAnterior))
            {
                CambiarCamara(-1);
            }
        }
    }

    void EquiparTablet()
    {
        estaEquipada = true;
        ActivarColliders(false);

        transform.SetParent(manoJugador);
        transform.localPosition = posicionEnMano;
        transform.localRotation = Quaternion.Euler(rotacionEnMano);
    }

    void SoltarTablet()
    {
        estaEquipada = false;

        transform.SetParent(padreOriginal);
        transform.localPosition = posicionOriginal;
        transform.localRotation = rotacionOriginal;

        ActivarColliders(true);
    }

    // Evita que los colliders de la tablet empujen al jugador mientras la tiene en la mano
    void ActivarColliders(bool activo)
    {
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = activo;
        }
    }

    void CambiarCamara(int direccion)
    {
        indiceCamaraActual += direccion;

        if (indiceCamaraActual >= camaras.Length)
            indiceCamaraActual = 0;
        else if (indiceCamaraActual < 0)
            indiceCamaraActual = camaras.Length - 1;

        ActivarCamaraActual();
    }

    void ActivarCamaraActual()
    {
        for (int i = 0; i < camaras.Length; i++)
        {
            if (camaras[i] != null && camaras[i].povCamera != null)
            {
                bool esLaActiva = (i == indiceCamaraActual);
                camaras[i].povCamera.targetTexture = esLaActiva ? cameraFeedRT : null;
                camaras[i].povCamera.enabled = esLaActiva;
            }
        }
    }
}