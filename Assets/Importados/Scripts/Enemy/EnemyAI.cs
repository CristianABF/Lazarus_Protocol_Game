using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum TargetType { CoreType, PlayerHunter }

    [Header("Tipo de Comportamiento")]
    [Tooltip("CoreType = Va al Núcleo (Skulk). PlayerHunter = Busca y merodea al Jugador (Marauder/Bulwark).")]
    [SerializeField] private TargetType enemyType = TargetType.CoreType;

    [Header("Rango y Detección")]
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float attackDistance = 2.5f;
    [SerializeField] private float damage = 10f;

    [Header("Ruta de Zonas (Patrulla) - Solo Hunters")]
    [Tooltip("Lista de puntos/zonas por las que los enemigos irán transitando.")]
    [SerializeField] private List<Transform> patrolZones = new List<Transform>();
    [SerializeField] private float zoneArrivalRadius = 3f; // Distancia para considerar que "llegó" a la zona
    [SerializeField] private float wanderRadius = 6f;       // Radio para merodear DENTRO de la zona actual
    [SerializeField] private float wanderTimer = 4f;        // Tiempo entre pequeños pasos al merodear

    private Transform currentZoneTarget;
    private float timerWander;
    private bool isWanderingInZone = false;

    [Header("Attack Speed")]
    [SerializeField] private float attackRate = 2f;
    private float lastAttack = 0f;

    [Header("Reload")]
    [SerializeField] private float reloadSpeed = 3.3f;
    [SerializeField] private int ammoAmount = 5;
    [SerializeField] private int reloadAmount = 5;

    [Header("Merodeo alrededor del Núcleo")]
    [SerializeField] private float coreWanderRadius = 6f;

    private NavMeshAgent navMeshAgent;
    private float distanceToTarget = Mathf.Infinity;
    private bool isProvoked = false;
    private bool isReloading = false;
    private bool isStunned = false;

    private Transform playerTarget;
    private Transform coreTarget;
    private Transform currentTarget;
    private CoreBehaviour coreScript;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        // Evitación física dinámica para que no colisionen en bloque
        navMeshAgent.avoidancePriority = Random.Range(30, 80);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;

        GameObject coreObj = GameObject.FindGameObjectWithTag("Core");
        if (coreObj != null)
        {
            coreTarget = coreObj.transform;
            coreScript = coreObj.GetComponent<CoreBehaviour>();
        }

        timerWander = wanderTimer;

        if (enemyType == TargetType.CoreType)
        {
            currentTarget = coreTarget;
        }
        else
        {
            currentTarget = playerTarget;

            // Carga automática de zonas si no están asignadas manualmente
            if (patrolZones.Count == 0)
            {
                BuscarZonasDePatrullaEnEscena();
            }

            // Selecciona la primera zona aleatoria al hacer Spawn
            SeleccionarSiguienteZona();
        }
    }

    private void Update()
    {
        if (isStunned) return;

        // Si es un Hunter y no ha detectado al jugador, navega por la red de zonas
        if (enemyType == TargetType.PlayerHunter && !isProvoked)
        {
            PatrullarYMerodear();
            CheckPlayerProximity();
            return;
        }

        if (currentTarget == null) return;

        distanceToTarget = Vector3.Distance(currentTarget.position, transform.position);

        if (isProvoked)
        {
            if (coreScript != null) coreScript.LiberarLugar(transform);
            EngageTarget();

            if (currentTarget == playerTarget && distanceToTarget > chaseRange * 2f)
            {
                isProvoked = false;

                if (enemyType == TargetType.CoreType)
                {
                    currentTarget = coreTarget;
                }
                else
                {
                    // Al perder al jugador, retoma la marcha hacia una zona de patrulla
                    SeleccionarSiguienteZona();
                }
            }
        }
        else if (enemyType == TargetType.CoreType)
        {
            if (currentTarget != coreTarget) currentTarget = coreTarget;

            bool tieneCupo = coreScript != null && coreScript.SolicitarnLugarParaAtacar(transform);

            if (tieneCupo)
            {
                if (distanceToTarget > attackDistance)
                {
                    ChaseTarget();
                }
                else
                {
                    Wait();
                    ShootTarget();
                }
            }
            else
            {
                WanderAroundCore();
            }
        }
    }

    private void PatrullarYMerodear()
    {
        if (currentZoneTarget == null)
        {
            SeleccionarSiguienteZona();
            return;
        }

        float distToZone = Vector3.Distance(transform.position, currentZoneTarget.position);

        // 1. Si está en camino hacia la zona actual
        if (!isWanderingInZone)
        {
            navMeshAgent.SetDestination(currentZoneTarget.position);
            SetRunningAnim(true);

            // ¿Llegó a la zona objetivo?
            if (distToZone <= zoneArrivalRadius)
            {
                isWanderingInZone = true;
                timerWander = wanderTimer; // Fuerza el primer paso de merodeo
            }
        }
        // 2. Si ya llegó a la zona, merodea un poco antes de ir a la siguiente
        else
        {
            timerWander += Time.deltaTime;

            if (timerWander >= wanderTimer)
            {
                // Un par de merodeos pequeños en la zona actual
                Vector3 newWanderPos = RandomNavSphere(currentZoneTarget.position, wanderRadius, -1);
                navMeshAgent.SetDestination(newWanderPos);
                SetRunningAnim(true);
                timerWander = 0f;

                // Probabilidad de cambiar de zona tras merodear un tiempo
                if (Random.value > 0.5f)
                {
                    isWanderingInZone = false;
                    SeleccionarSiguienteZona();
                }
            }

            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                SetRunningAnim(false);
            }
        }
    }

    private void SeleccionarSiguienteZona()
    {
        if (patrolZones.Count == 0) return;

        // Elegir una zona aleatoria diferente a la actual si hay varias disponibles
        Transform nuevaZona = patrolZones[Random.Range(0, patrolZones.Count)];

        if (patrolZones.Count > 1 && nuevaZona == currentZoneTarget)
        {
            int index = (patrolZones.IndexOf(nuevaZona) + 1) % patrolZones.Count;
            nuevaZona = patrolZones[index];
        }

        currentZoneTarget = nuevaZona;
        isWanderingInZone = false;

        if (currentZoneTarget != null)
        {
            navMeshAgent.SetDestination(currentZoneTarget.position);
            SetRunningAnim(true);
        }
    }

    private void BuscarZonasDePatrullaEnEscena()
    {
        // Busca automáticamente en la escena todos los GameObjects etiquetados como "PatrolZone"
        GameObject[] zonas = GameObject.FindGameObjectsWithTag("PatrolZone");
        foreach (GameObject z in zonas)
        {
            patrolZones.Add(z.transform);
        }
    }

    private void CheckPlayerProximity()
    {
        if (playerTarget == null) return;

        float distToPlayer = Vector3.Distance(playerTarget.position, transform.position);
        if (distToPlayer <= chaseRange)
        {
            isProvoked = true;
            currentTarget = playerTarget;
        }
    }

    private void WanderAroundCore()
    {
        if (coreTarget == null) return;

        timerWander += Time.deltaTime;

        if (timerWander >= wanderTimer)
        {
            Vector3 randomPointAroundCore = RandomNavSphere(coreTarget.position, coreWanderRadius, -1);
            navMeshAgent.SetDestination(randomPointAroundCore);
            SetRunningAnim(true);
            timerWander = 0f;
        }

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            SetRunningAnim(false);
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randDirection = Random.insideUnitSphere * dist;
            randDirection += origin;

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(randDirection, out navHit, dist, layermask))
            {
                return navHit.position;
            }
        }
        return origin;
    }

    public void OnDamageTaken(Transform attacker = null)
    {
        if (attacker == null) return;

        if (attacker.CompareTag("Player") || attacker.GetComponentInParent<PlayerBehaviour>() != null)
        {
            currentTarget = playerTarget;
            isProvoked = true;
        }
    }

    private void EngageTarget()
    {
        FaceTarget();

        if (!isReloading)
        {
            if (distanceToTarget > attackDistance)
            {
                ChaseTarget();
            }
            else
            {
                Wait();
                ShootTarget();
            }
        }
    }

    private void ShootTarget()
    {
        if (Time.time > lastAttack + attackRate)
        {
            lastAttack = Time.time;

            if (ammoAmount > 0)
            {
                AplicarDañoAlObjetivo();
                GetComponentInChildren<Animator>().SetTrigger("shoot");
                ammoAmount--;
            }
            else if (ammoAmount <= 0)
            {
                Wait();
                Reload();
            }
        }
    }

    private void AplicarDañoAlObjetivo()
    {
        if (currentTarget.CompareTag("Player"))
        {
            PlayerBehaviour jugador = currentTarget.GetComponent<PlayerBehaviour>();
            if (jugador != null) jugador.RecibirDano(damage);
        }
        else if (currentTarget.CompareTag("Core"))
        {
            CoreBehaviour core = currentTarget.GetComponent<CoreBehaviour>();
            if (core != null) core.RecibirDaño(damage);
        }
    }

    private void Reload()
    {
        if (!GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Reload"))
        {
            isReloading = true;
            GetComponentInChildren<Animator>().SetTrigger("reload");
            StartCoroutine(ReloadAnimation());
        }
    }

    private IEnumerator ReloadAnimation()
    {
        yield return new WaitForSeconds(reloadSpeed);
        ammoAmount = reloadAmount;
        isReloading = false;
    }

    public void TargetDetected()
    {
        isProvoked = true;
        currentTarget = playerTarget;
    }

    public void SetStunned(bool state)
    {
        isStunned = state;

        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = state;
        }

        SetRunningAnim(false);
    }

    private void Wait()
    {
        SetRunningAnim(false);
        navMeshAgent.isStopped = true;
    }

    private void ChaseTarget()
    {
        SetRunningAnim(true);
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(currentTarget.position);
    }

    private void FaceTarget()
    {
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void SetRunningAnim(bool state)
    {
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.SetBool("isRunning", state);
    }

    private void OnDestroy()
    {
        if (coreScript != null)
        {
            coreScript.LiberarLugar(transform);
        }
    }
}