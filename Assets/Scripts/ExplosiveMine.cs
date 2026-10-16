using UnityEngine;

public class ExplosiveMine : Landmine
{
    [Header("Ajustes de Explosión")]
    [SerializeField] private float damage = 50f;
    [SerializeField] private float explosionRadius = 4f; // Radio del daño en área
    [SerializeField] private float vfxLifetime = 2f;

    protected override void OnDetonate(GameObject target)
    {
        // 1. Instanciar VFX y Audio
        if (explosionEffect != null)
        {
            GameObject vfxInstance = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(vfxInstance, vfxLifetime);
        }

        if (soundEffect != null)
        {
            AudioSource.PlayClipAtPoint(soundEffect, transform.position, 1.0f);
        }

        // 2. Detectar a TODOS los enemigos dentro del radio de explosión
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayer);

        foreach (Collider hit in hitColliders)
        {
            EnemyHealth enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }

        // 3. Destruir la mina
        Destroy(gameObject);
    }

    // Dibujar el radio de explosión en la vista de Scene para ajustarlo visualmente
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}