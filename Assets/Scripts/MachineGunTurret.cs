using UnityEngine;

public class MachineGunTurret : TurretBase
{
    [Header("Parametros Metralleta")]
    [SerializeField] private float damage = 10f;

    protected override void Attack()
    {
        if (target == null) return;

        // Disparo mediante Raycast (o instanciar prefab de Bala)
        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, range, enemyLayer))
        {
            // Reemplaza 'IDamageable' o 'EnemyHealth' según el script en uso
            if (hit.collider.TryGetComponent(out IDamageable enemy))
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}
