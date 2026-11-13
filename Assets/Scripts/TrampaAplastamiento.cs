using UnityEngine;

public class TrampaAplastamiento : MonoBehaviour
{
    private PuertaCorrediza puertaPadre;

    void Start()
    {
        puertaPadre = GetComponentInParent<PuertaCorrediza>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (puertaPadre == null || !puertaPadre.estaFallando) return;

        if (puertaPadre.cooldownDano <= 0)
        {
            if (other.CompareTag("Player"))
            {
                PlayerBehaviour jugador = other.GetComponent<PlayerBehaviour>();
                if (jugador != null)
                {
                    jugador.RecibirDano(puertaPadre.danoPorAplastamiento);
                    puertaPadre.cooldownDano = 1f;
                }
            }
            else if (other.CompareTag("Enemy") || other.transform.root.CompareTag("Enemy"))
            {
                EnemyHealth enemigo = other.GetComponentInParent<EnemyHealth>();
                if (enemigo != null)
                {
                    enemigo.TakeDamage(puertaPadre.danoPorAplastamiento);
                    puertaPadre.cooldownDano = 1f;
                }
            }
        }
    }
}