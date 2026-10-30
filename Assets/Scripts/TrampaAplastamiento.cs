using UnityEngine;

public class TrampaAplastamiento : MonoBehaviour
{
    private PuertaCorrediza puertaPadre;

    void Start()
    {
        // El hijo busca al padre que contiene la lógica general
        puertaPadre = GetComponentInParent<PuertaCorrediza>();
    }

    private void OnTriggerStay(Collider other)
    {
        // 1. Si el padre no está fallando, no hacemos daño
        if (puertaPadre == null || !puertaPadre.estaFallando) return;

        // 2. Solo hacemos daño si el padre tiene cooldown en 0
        if (puertaPadre.cooldownDano <= 0)
        {
            if (other.CompareTag("Player"))
            {
                PlayerBehaviour jugador = other.GetComponent<PlayerBehaviour>();
                if (jugador != null)
                {
                    jugador.RecibirDano(puertaPadre.danoPorAplastamiento);
                    // Reiniciamos el cooldown en el padre
                    puertaPadre.cooldownDano = 1f;
                }
            }
            else if (other.CompareTag("Enemy") || other.transform.root.CompareTag("Enemy"))
            {
                // Busca en el padre del objeto que chocó (por si la bala le da a un brazo del enemigo)
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