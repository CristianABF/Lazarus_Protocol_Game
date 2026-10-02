using UnityEngine;

public class SniperTurret : TurretBase
{
    [Header("Parametros Sniper")]
    [SerializeField] private float highDamage = 80f;

    protected override void Attack()
    {
        if (target == null) return;

        // Disparo de alto impacto a gran distancia
        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, range, enemyLayer))
        {
            if (hit.collider.TryGetComponent(out IDamageable enemy))
            {
                enemy.TakeDamage(highDamage);
            }
        }
    }
}
