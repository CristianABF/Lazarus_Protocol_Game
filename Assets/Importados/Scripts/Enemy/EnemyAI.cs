using System;
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
    [SerializeField] private float chaseRange = 10f; // Rango de visión/detección
    [SerializeField] private float attackDistance = 2.5f;
    [SerializeField] private float damage = 10f;

    [Header("Merodeo (Wander) - Solo para Hunters")]
    [SerializeField] private float wanderRadius = 15f;
    [SerializeField] private float wanderTimer = 5f;
    private float timerWander;
    [Header("Merodeo alrededor del Núcleo")]
    [SerializeField] private float coreWanderRadius = 6f;
    private CoreBehaviour coreScript;

    [Header("Attack Speed")]
    [SerializeField] private float attackRate = 2f;
    private float lastAttack = 0f;

    [Header("Reload")]
    [SerializeField] private float reloadSpeed = 3.3f;
    [SerializeField] private int ammoAmount = 5;
    [SerializeField] private int reloadAmount = 5;

    private NavMeshAgent navMeshAgent;
    private float distanceToTarget = Mathf.Infinity;
    private bool isProvoked = false;
    private bool isReloading = false;
    private bool isStunned = false;

    private Transform playerTarget;
    private Transform coreTarget;
    private Transform currentTarget;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

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
        }
    }

    private void Update()
    {
        if (isStunned) return;

        if (enemyType == TargetType.PlayerHunter && !isProvoked)
        {
            Wander(transform.position, wanderRadius);
            CheckPlayerProximity();
            return;
        }

        if (currentTarget == null) return;

        distanceToTarget = Vector3.Distance(currentTarget.position, transform.position);

        if (isProvoked)
        {
            // Si el enemigo atacaba el núcleo y cambia al jugador, libera el cupo
            if (coreScript != null) coreScript.LiberarLugar(transform);
            EngageTarget();

            if (currentTarget == playerTarget && distanceToTarget > chaseRange * 2f)
            {
                isProvoked = false;
                if (enemyType == TargetType.CoreType) currentTarget = coreTarget;
                else navMeshAgent.ResetPath();
            }
        }
        else if (enemyType == TargetType.CoreType)
        {
            if (currentTarget != coreTarget) currentTarget = coreTarget;

            // Comprobamos si tiene cupo para atacar
            bool tieneCupo = coreScript != null && coreScript.SolicitarnLugarParaAtacar(transform);

            if (tieneCupo)
            {
                // Hay lugar: avanza a rango de ataque y golpea
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
                // No hay cupo: merodea EN VÍSPERAS del núcleo sin alejarse
                WanderAroundCore();
            }
        }
    }

    private void WanderAroundCore()
    {
        if (coreTarget == null) return;

        timerWander += Time.deltaTime;

        if (timerWander >= wanderTimer)
        {
            // Busca un punto aleatorio pero usando la posición del NÚCLEO como centro
            Vector3 randomPointAroundCore = RandomNavSphere(coreTarget.position, coreWanderRadius, -1);
            navMeshAgent.SetDestination(randomPointAroundCore);
            GetComponentInChildren<Animator>().SetBool("isRunning", true);
            timerWander = 0f;
        }

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            GetComponentInChildren<Animator>().SetBool("isRunning", false);
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

    private void Wander(Vector3 origin, float radius)
    {
        timerWander += Time.deltaTime;

        if (timerWander >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(origin, radius, -1);
            navMeshAgent.SetDestination(newPos);
            GetComponentInChildren<Animator>().SetBool("isRunning", true);
            timerWander = 0f;
        }

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            GetComponentInChildren<Animator>().SetBool("isRunning", false);
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = UnityEngine.Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randDirection, out navHit, dist, layermask))
        {
            return navHit.position;
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

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.SetBool("isRunning", false);
        }
    }

    private void Wait()
    {
        GetComponentInChildren<Animator>().SetBool("isRunning", false);
        navMeshAgent.isStopped = true;
    }

    private void ChaseTarget()
    {
        GetComponentInChildren<Animator>().SetBool("isRunning", true);
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(currentTarget.position);
    }

    private void FaceTarget()
    {
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void OnDestroy()
    {
        // Liberamos el cupo del núcleo de forma segura al destruir el objeto
        if (coreScript != null)
        {
            coreScript.LiberarLugar(transform);
        }
    }
}