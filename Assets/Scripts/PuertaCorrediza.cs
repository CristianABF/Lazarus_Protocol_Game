using UnityEngine;

public class PuertaCorrediza : MonoBehaviour
{
    [Header("Estado de la Puerta")]
    public bool estaRota = false;     // Se queda quieta y bloqueada permanentemente
    public bool estaFallando = false; // Se abre y cierra sola bruscamente, haciendo daño

    [Header("Partes Móviles (Arrastrar desde Hierarchy)")]
    public Transform puertaIzquierda;
    public Transform puertaDerecha;

    [Header("Ajustes de Movimiento")]
    public float distanciaApertura = 1.5f;
    public float velocidad = 5f;

    [Header("Ajustes de Fallo (Daño)")]
    public float danoPorAplastamiento = 25f;
    public float ritmoFallo = 0.5f; // Cada cuánto tiempo invierte su dirección

    private bool estaAbierta = false;
    private Vector3 posCerradaIzq, posAbiertaIzq;
    private Vector3 posCerradaDer, posAbiertaDer;

    private float timerFallo;
    public float cooldownDano = 0f; // Para que no haga 60 veces de daño por segundo

    void Start()
    {
        if (puertaIzquierda != null && puertaDerecha != null)
        {
            posCerradaIzq = puertaIzquierda.localPosition;
            posCerradaDer = puertaDerecha.localPosition;

            // Recuerda cambiar left/right por forward/back si tu puerta se abre hacia el eje Z
            posAbiertaIzq = posCerradaIzq + (Vector3.forward * distanciaApertura);
            posAbiertaDer = posCerradaDer + (Vector3.back * distanciaApertura);
        }
    }

    void Update()
    {
        if (puertaIzquierda == null || puertaDerecha == null) return;

        // --- SISTEMA ERRÁTICO (FALLO) ---
        if (estaFallando)
        {
            timerFallo -= Time.deltaTime;
            if (timerFallo <= 0)
            {
                estaAbierta = !estaAbierta; // Invierte el estado bruscamente
                // Le sumamos un valor aleatorio para que el fallo no sea un reloj perfecto
                timerFallo = ritmoFallo + Random.Range(-0.2f, 0.4f);
            }
        }

        // Reloj de gracia para el daño
        if (cooldownDano > 0) cooldownDano -= Time.deltaTime;

        Vector3 destinoIzq = estaAbierta ? posAbiertaIzq : posCerradaIzq;
        Vector3 destinoDer = estaAbierta ? posAbiertaDer : posCerradaDer;

        // Si está fallando, multiplicamos la velocidad para que los "golpes" de la puerta sean más violentos
        float velActual = estaFallando ? velocidad * 4f : velocidad;

        puertaIzquierda.localPosition = Vector3.Lerp(puertaIzquierda.localPosition, destinoIzq, Time.deltaTime * velActual);
        puertaDerecha.localPosition = Vector3.Lerp(puertaDerecha.localPosition, destinoDer, Time.deltaTime * velActual);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si la puerta está rota o fallando, ignora por completo la presencia para abrirse amablemente
        if (estaRota || estaFallando) return;

        if (other.CompareTag("Player")) estaAbierta = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (estaRota || estaFallando) return;

        if (other.CompareTag("Player")) estaAbierta = false;
    }

}