using UnityEngine;

public abstract class TurretBase : MonoBehaviour
{
    [Header("Configuración Base")]
    [SerializeField] protected Transform turretHead; // Parte de la torreta que rota
    [SerializeField] protected Transform firePoint;  // Punto desde donde sale el disparo
    [SerializeField] protected LayerMask enemyLayer; // Capa de los enemigos

    [Header("Atributos de Torreta")]
    [SerializeField] protected float range = 15f;          // Alcance de detección
    [SerializeField] protected float rotationSpeed = 5f;  // Velocidad de giro
    [SerializeField] protected float fireRate = 1f;       // Disparos o pulsos por segundo

    [Header("Ajuste de Orientación")]
    [Tooltip("Ajusta si el modelo 3D no apunta de frente al objetivo (ej. 90, -90, 180)")]
    [SerializeField] protected float rotationYOffset = 0f;

    protected Transform target;
    protected float fireCountdown = 0f;

    protected virtual void Start()
    {
        // Busca objetivos periódicamente para optimizar rendimiento
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
    }

    protected virtual void Update()
    {
        if (target == null) return;

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
            // Comprobación por tag "Enemy"
            if (!enemyCollider.CompareTag("Enemy") && !enemyCollider.transform.root.CompareTag("Enemy"))
                continue;

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
        if (target == null || turretHead == null) return;

        Vector3 dir = target.position - turretHead.position;
        dir.y = 0f; // Forzamos el plano horizontal

        if (dir == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Quaternion offsetRotation = lookRotation * Quaternion.Euler(0f, rotationYOffset, 0f);

        turretHead.rotation = Quaternion.Slerp(turretHead.rotation, offsetRotation, Time.deltaTime * rotationSpeed);
    }

    // Cada tipo de torreta implementa su propia lógica de ataque
    protected abstract void Attack();

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}