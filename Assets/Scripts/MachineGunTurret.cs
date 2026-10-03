using UnityEngine;

public class MachineGunTurret : TurretBase
{
    [Header("Parámetros Metralleta")]
    [SerializeField] private float damage = 10f;

    protected override void Attack()
    {
        if (target == null || firePoint == null) return;

        Vector3 direction = (target.position - firePoint.position).normalized;

        if (Physics.Raycast(firePoint.position, direction, out RaycastHit hit, range, enemyLayer))
        {
            if (hit.transform.CompareTag("Enemy") || hit.transform.root.CompareTag("Enemy"))
            {
                Component damageable = hit.transform.GetComponentInParent(typeof(IDamageable));
                if (damageable != null)
                {
                    GameFunctions.Attack(damageable, damage);
                }

                // Dispara partículas o efectos de impacto en el enemigo si los tiene
                ParticleSystem ps = hit.transform.GetComponentInChildren<ParticleSystem>();
                if (ps != null) ps.Play();

                // Notifica a la IA del enemigo
                EnemyAI enemy = hit.transform.GetComponentInParent<EnemyAI>();
                if (enemy != null) enemy.OnDamageTaken();
            }
        }
    }
}