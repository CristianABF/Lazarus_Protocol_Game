using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletCameraManager : MonoBehaviour
{
    [Header("Configuraci�n de Transmisi�n")]
    public RenderTexture cameraFeedRT;
    public List<Camera> camaras = new List<Camera>();

    [Header("Interacci�n y Controles")]
    public KeyCode teclaInteractuar = KeyCode.E;
    public KeyCode teclaCamAnterior = KeyCode.A;
    public KeyCode teclaCamSiguiente = KeyCode.D;
    public Transform manoJugador;
    public float distanciaInteraccion = 2.0f;

    [Header("Posici�n en la mano (valores locales)")]
    public Vector3 posicionEnMano = new Vector3(0f, -0.1f, 0.7f);
    public Vector3 rotacionEnMano = Vector3.zero;

    private int indiceCamaraActual = 0;
    private bool estaEquipada = false;
    private Rigidbody rb;
    private Collider col;

    private void Start()
    {
        // Obtener referencias de f�sica
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Si se agreg� Rigidbody por editor, aseguramos que inicie preparado para caer si est� en la mesa
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Inicializar transmisi�n en la primera c�mara si existen
        if (camaras.Count > 0)
        {
            ActivarCamara(0);
        }
    }

    private void Update()
    {
        // Detectar interacci�n si est� cerca del jugador o equipada
        if (Input.GetKeyDown(teclaInteractuar))
        {
            if (estaEquipada)
            {
                SoltarTablet();
            }
            else
            {
                // Verificar si el jugador est� lo suficientemente cerca
                if (manoJugador != null && Vector3.Distance(transform.position, manoJugador.position) <= distanciaInteraccion)
                {
                    EquiparTablet();
                }
            }
        }

        // Cambiar entre c�maras si la tablet est� en la mano
        if (estaEquipada && camaras.Count > 0)
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

    public void EquiparTablet()
    {
        estaEquipada = true;

        // 1. Desactivar f�sicas mientras se sostiene
        if (rb != null)
        {
            rb.isKinematic = true; // Desactiva la gravedad y colisiones din�micas
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 2. Emparentar a la mano del jugador
        transform.SetParent(manoJugador);
        transform.localPosition = posicionEnMano;
        transform.localRotation = Quaternion.Euler(rotacionEnMano);
    }

    public void SoltarTablet()
    {
        estaEquipada = false;

        // 1. Desemparentar de la mano
        transform.SetParent(null);

        // 2. Activar f�sicas para que caiga por peso
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    private void CambiarCamara(int direccion)
    {
        indiceCamaraActual += direccion;

        if (indiceCamaraActual >= camaras.Count)
        {
            indiceCamaraActual = 0;
        }
        else if (indiceCamaraActual < 0)
        {
            indiceCamaraActual = camaras.Count - 1;
        }

        ActivarCamara(indiceCamaraActual);
    }

    private void ActivarCamara(int indice)
    {
        for (int i = 0; i < camaras.Count; i++)
        {
            if (camaras[i] != null)
            {
                if (i == indice)
                {
                    camaras[i].targetTexture = cameraFeedRT;
                    camaras[i].enabled = true;
                }
                else
                {
                    camaras[i].targetTexture = null;
                    camaras[i].enabled = false;
                }
            }
        }
    }
}