using System.Collections;
using UnityEngine;

public class ElectricMine : Landmine
{
    [Header("Ajustes Eléctricos")]
    [SerializeField] private float totalDamage = 30f; // 10 de daño por pulso
    [SerializeField] private float timeBetweenTicks = 0.5f;

    protected override void OnDetonate(GameObject target)
    {
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        if (soundEffect != null)
            AudioSource.PlayClipAtPoint(soundEffect, transform.position);

        // Desactivar visibilidad y colisiones para que no vuelva a activarse ni estorbe
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        StartCoroutine(ElectricDamageRoutine(target));
    }

    private IEnumerator ElectricDamageRoutine(GameObject target)
    {
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        EnemyAI enemyAI = target.GetComponent<EnemyAI>();

        // Activar Stun en la IA del enemigo
        if (enemyAI != null)
        {
            enemyAI.SetStunned(true);
        }

        float tickDamage = totalDamage / 3f;

        // Pulso 1, 2 y 3
        for (int i = 0; i < 3; i++)
        {
            if (target != null && enemyHealth != null)
            {
                enemyHealth.TakeDamage(tickDamage);
            }
            yield return new WaitForSeconds(timeBetweenTicks);
        }

        // Quitar Stun y destruir la mina
        if (target != null && enemyAI != null)
        {
            enemyAI.SetStunned(false);
        }

        Destroy(gameObject);
    }
}