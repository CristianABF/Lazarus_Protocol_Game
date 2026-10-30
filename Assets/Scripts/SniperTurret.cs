using UnityEngine;

public class SniperTurret : TurretBase
{
    [Header("Parámetros Sniper")]
    [SerializeField] private float highDamage = 80f;
    [SerializeField] private GameObject bulletPrefab;

    protected override void Attack()
    {
        if (target == null || firePoint == null || bulletPrefab == null) return;

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Vector3 direction = (target.position - firePoint.position).normalized;
        bulletGO.transform.rotation = Quaternion.LookRotation(direction);

        if (bulletGO.TryGetComponent(out Bullet bullet))
        {
            bullet.Setup(highDamage);
        }
    }
    protected override void Start()
    {
        base.Start(); // Obligatorio para ejecutar la lógica de recarga del padre

        if (ControladorPartida.Instancia != null)
        {
            highDamage *= ControladorPartida.Instancia.factorDañoTorretas;
        }
    }
}