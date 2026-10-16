using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricMine : Landmine
{
    [Header("Ajustes Eléctricos")]
    [SerializeField] private float totalDamage = 30f;
    [SerializeField] private float stunRadius = 4f; // Radio del choque eléctrico
    [SerializeField] private float timeBetweenTicks = 0.5f;
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

        // 2. Ocultar la mina y desactivar colisiones
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) r.enabled = false;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders) c.enabled = false;

        // 3. Iniciar rutina de daño y stun en área
        StartCoroutine(ElectricAoERoutine());
    }

    private IEnumerator ElectricAoERoutine()
    {
        // Escanear enemigos dentro del radio
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, stunRadius, enemyLayer);

        List<EnemyHealth> enemiesHealth = new List<EnemyHealth>();
        List<EnemyAI> enemiesAI = new List<EnemyAI>();

        // Agrupar referencias únicas de enemigos
        foreach (Collider hit in hitColliders)
        {
            EnemyHealth health = hit.GetComponentInParent<EnemyHealth>();
            EnemyAI ai = hit.GetComponentInParent<EnemyAI>();

            if (health != null && !enemiesHealth.Contains(health))
            {
                enemiesHealth.Add(health);
            }

            if (ai != null && !enemiesAI.Contains(ai))
            {
                enemiesAI.Add(ai);
                ai.SetStunned(true); // Activar Stun a todos
            }
        }

        float tickDamage = totalDamage / 3f;

        // 3 Pulsos de daño para todos los enemigos atrapados
        for (int i = 0; i < 3; i++)
        {
            foreach (EnemyHealth enemy in enemiesHealth)
            {
                if (enemy != null)
                {
                    enemy.TakeDamage(tickDamage);
                }
            }
            yield return new WaitForSeconds(timeBetweenTicks);
        }

        // Remover Stun a todos los enemigos
        foreach (EnemyAI ai in enemiesAI)
        {
            if (ai != null)
            {
                ai.SetStunned(false);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stunRadius);
    }
}