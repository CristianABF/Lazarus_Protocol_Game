using UnityEngine;

public class ExplosiveMine : Landmine
{
    [Header("Ajustes de Explosion")]
    [SerializeField] private float damage = 50f;

    protected override void OnDetonate(GameObject target)
    {
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        if (soundEffect != null)
            AudioSource.PlayClipAtPoint(soundEffect, transform.position);

        // Aplica el daño al enemigo
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
