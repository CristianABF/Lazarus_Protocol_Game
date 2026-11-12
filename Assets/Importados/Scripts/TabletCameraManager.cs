using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletCameraManager : MonoBehaviour
{
    [Header("Configuración de Transmisión")]
    public RenderTexture cameraFeedRT;
    public List<Camera> camaras = new List<Camera>();

    [Header("Interacción y Controles")]
    public KeyCode teclaInteractuar = KeyCode.C;
    public KeyCode teclaCamAnterior = KeyCode.A;
    public KeyCode teclaCamSiguiente = KeyCode.D;
    public Transform manoJugador;
    public float distanciaInteraccion = 5.0f;

    [Header("Posición en la mano (valores locales)")]
    public Vector3 posicionEnMano = new Vector3(0f, -0.1f, 0.7f);
    public Vector3 rotacionEnMano = Vector3.zero;

    private int indiceCamaraActual = 0;
    private bool estaEquipada = false;
    private Rigidbody rb;
    private Collider col;

    private void Start()
    {
        // Obtener referencias de física
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Asegurar que inicie preparado para caer si está en la mesa
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Inicializar transmisión en la primera cámara si existen
        if (camaras.Count > 0)
        {
            ActivarCamara(0);
        }
    }

    private void Update()
    {
        // Si la referencia de la mano está vacía o rota (Missing), la busca en la escena
        if (manoJugador == null)
        {
            // 1. Intenta buscar por nombre exacto o con espacio
            GameObject manoEnEscena = GameObject.Find("ManoJugador") ?? GameObject.Find("Mano Jugador");

            // 2. Si no existe con ese nombre, usa la cámara principal del jugador
            if (manoEnEscena != null)
            {
                manoJugador = manoEnEscena.transform;
            }
            else if (Camera.main != null)
            {
                manoJugador = Camera.main.transform;
            }
        }

        // Detectar interacción si está cerca del jugador o equipada
        if (Input.GetKeyDown(teclaInteractuar))
        {
            if (estaEquipada)
            {
                SoltarTablet();
            }
            else
            {
                // Verificar si el jugador está lo suficientemente cerca
                if (manoJugador != null && Vector3.Distance(transform.position, manoJugador.position) <= distanciaInteraccion)
                {
                    EquiparTablet();
                }
            }
        }

        // Cambiar entre cámaras si la tablet está en la mano
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

        // 1. Desactivar físicas mientras se sostiene
        if (rb != null)
        {
            rb.isKinematic = true;
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

        // 2. Activar físicas para que caiga por peso
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