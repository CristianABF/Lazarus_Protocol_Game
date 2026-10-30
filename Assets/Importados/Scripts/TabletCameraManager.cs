using UnityEngine;

public class TabletCameraManager : MonoBehaviour
{
    [Header("Configuración de Transmisión")]
    public RenderTexture cameraFeedRT;
    public SecurityCameraControl[] camaras;

    [Header("Interacción con el Jugador")]
    public KeyCode teclaInteractuar = KeyCode.E;
    public Transform manoJugador;
    public float distanciaInteraccion = 3f;

    private Transform jugadorTransform;
    private int indiceCamaraActual = 0;
    private bool sostieneTablet = false;

    private Vector3 posicionOriginal;
    private Quaternion rotacionOriginal;
    private Transform padreOriginal;

    void Start()
    {
        posicionOriginal = transform.position;
        rotacionOriginal = transform.rotation;
        padreOriginal = transform.parent;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            jugadorTransform = playerObj.transform;
        }

        DesactivarTodasLasCamaras();
    }

    void Update()
    {
        DeteccionInteractuar();

        if (sostieneTablet)
        {
            NavegacionCamaras();
        }
    }

    void DeteccionInteractuar()
    {
        if (Input.GetKeyDown(teclaInteractuar))
        {
            if (sostieneTablet)
            {
                SoltarTablet();
            }
            else
            {
                if (jugadorTransform != null)
                {
                    float distancia = Vector3.Distance(transform.position, jugadorTransform.position);
                    if (distancia <= distanciaInteraccion)
                    {
                        TomarTablet();
                    }
                }
            }
        }
    }

    void TomarTablet()
    {
        sostieneTablet = true;

        if (manoJugador != null)
        {
            transform.SetParent(manoJugador);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        ActivarCamaraPorIndice(indiceCamaraActual);
    }

    void SoltarTablet()
    {
        sostieneTablet = false;

        transform.SetParent(padreOriginal);
        transform.position = posicionOriginal;
        transform.rotation = rotacionOriginal;

        DesactivarTodasLasCamaras();
    }

    void NavegacionCamaras()
    {
        if (camaras == null || camaras.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.D))
        {
            indiceCamaraActual = (indiceCamaraActual + 1) % camaras.Length;
            ActivarCamaraPorIndice(indiceCamaraActual);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            indiceCamaraActual = (indiceCamaraActual - 1 + camaras.Length) % camaras.Length;
            ActivarCamaraPorIndice(indiceCamaraActual);
        }
    }

    void ActivarCamaraPorIndice(int indice)
    {
        DesactivarTodasLasCamaras();

        if (camaras != null && indice >= 0 && indice < camaras.Length)
        {
            if (camaras[indice] != null && camaras[indice].povCamera != null)
            {
                camaras[indice].povCamera.targetTexture = cameraFeedRT;
                camaras[indice].ActivarPOV(true);
            }
        }
    }

    void DesactivarTodasLasCamaras()
    {
        if (camaras == null) return;

        foreach (var cam in camaras)
        {
            if (cam != null && cam.povCamera != null)
            {
                cam.povCamera.targetTexture = null;
                cam.ActivarPOV(false);
            }
        }
    }
}