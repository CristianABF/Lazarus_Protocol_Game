using UnityEngine;

public class MachineGunTurret : TurretBase
{
    [Header("Parámetros Metralleta")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private GameObject bulletPrefab; // Prefab de la Bala

    protected override void Attack()
    {
        if (target == null || firePoint == null || bulletPrefab == null) return;

        // 1. Instanciar la bala en la posición y rotación del firePoint
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. Orientar la bala exactamente hacia la posición del enemigo (centro/pecho)
        Vector3 direction = (target.position - firePoint.position).normalized;
        bulletGO.transform.rotation = Quaternion.LookRotation(direction);

        // 3. Pasar el daño configurado en la torreta a la bala
        if (bulletGO.TryGetComponent(out Bullet bullet))
        {
            bullet.Setup(damage);
        }
    }
}