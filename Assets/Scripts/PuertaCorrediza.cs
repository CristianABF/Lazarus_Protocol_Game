using UnityEngine;

public class PuertaCorrediza : MonoBehaviour
{
    [Header("Estado de la Puerta")]
    public bool estaRota = false;
    public bool estaFallando = false;

    [Header("Partes Móviles (Arrastrar desde Hierarchy)")]
    public Transform puertaIzquierda;
    public Transform puertaDerecha;

    [Header("Ajustes de Movimiento")]
    public float distanciaApertura = 1.5f;
    public float velocidad = 5f;

    [Header("Ajustes de Fallo (Daño)")]
    public float danoPorAplastamiento = 25f;
    public float ritmoFallo = 0.5f;

    private bool estaAbierta = false;
    private Vector3 posCerradaIzq, posAbiertaIzq;
    private Vector3 posCerradaDer, posAbiertaDer;

    private float timerFallo;
    public float cooldownDano = 0f;

    void Start()
    {
        if (puertaIzquierda != null && puertaDerecha != null)
        {
            posCerradaIzq = puertaIzquierda.localPosition;
            posCerradaDer = puertaDerecha.localPosition;

            posAbiertaIzq = posCerradaIzq + (Vector3.forward * distanciaApertura);
            posAbiertaDer = posCerradaDer + (Vector3.back * distanciaApertura);
        }
    }

    void Update()
    {
        if (puertaIzquierda == null || puertaDerecha == null) return;

        if (estaFallando)
        {
            timerFallo -= Time.deltaTime;
            if (timerFallo <= 0)
            {
                estaAbierta = !estaAbierta;
                timerFallo = ritmoFallo + Random.Range(-0.2f, 0.4f);
            }
        }

        if (cooldownDano > 0) cooldownDano -= Time.deltaTime;

        Vector3 destinoIzq = estaAbierta ? posAbiertaIzq : posCerradaIzq;
        Vector3 destinoDer = estaAbierta ? posAbiertaDer : posCerradaDer;

        float velActual = estaFallando ? velocidad * 4f : velocidad;

        puertaIzquierda.localPosition = Vector3.Lerp(puertaIzquierda.localPosition, destinoIzq, Time.deltaTime * velActual);
        puertaDerecha.localPosition = Vector3.Lerp(puertaDerecha.localPosition, destinoDer, Time.deltaTime * velActual);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (estaRota || estaFallando) return;

        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            estaAbierta = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (estaRota || estaFallando) return;

        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            estaAbierta = false;
        }
    }
}