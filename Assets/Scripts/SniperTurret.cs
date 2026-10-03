using UnityEngine;

public class SniperTurret : TurretBase
{
    [Header("Parámetros Sniper")]
    [SerializeField] private float highDamage = 80f;

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
                    GameFunctions.Attack(damageable, highDamage);
                }

                ParticleSystem ps = hit.transform.GetComponentInChildren<ParticleSystem>();
                if (ps != null) ps.Play();

                EnemyAI enemy = hit.transform.GetComponentInParent<EnemyAI>();
                if (enemy != null) enemy.OnDamageTaken();
            }
        }
    }
}