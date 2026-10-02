using UnityEngine;

public abstract class TurretBase : MonoBehaviour
{
    [Header("Configuración Base")]
    [SerializeField] protected Transform turretHead; // Parte de la torreta que rota
    [SerializeField] protected Transform firePoint; // Punto desde donde se dispara/sale el ataque
    [SerializeField] protected LayerMask enemyLayer; // Capa de los enemigos

    [Header("Atributos de Torreta")]
    [SerializeField] protected float range = 15f; // Alcance de deteccion
    [SerializeField] protected float rotationSpeed = 5f; // Velocidad de giro
    [SerializeField] protected float fireRate = 1f; // Disparos o pulsos por segundo

    protected Transform target;
    protected float fireCountdown = 0f;

    protected virtual void Start()
    {
        // Busca objetivos periodicamente para optimizar rendimiento (2 veces por seg)
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
    }

    protected virtual void Update()
    {
        if (target != null) return;

        // Apuntar hacia el enemigo
        LockOnTarget();

        // Control de cadencia de disparo
        if (fireCountdown <= 0f)
        {
            Attack();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    protected virtual void UpdateTarget()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, range, enemyLayer);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (Collider enemyCollider in enemiesInRange)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemyCollider.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemyCollider.gameObject;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }

    protected virtual void LockOnTarget()
    {
        Vector3 dir = target.position - turretHead.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(turretHead.rotation, lookRotation, Time.deltaTime * rotationSpeed).eulerAngles;
        turretHead.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    // Cada tipo de torreta implementara su propia logica de ataque
    protected abstract void Attack();

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}