using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float attackDistance = 2.5f; // Renombrado para mayor claridad
    [SerializeField] private float damage = 10f;

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

    // Fragmentación del objetivo
    private Transform playerTarget;
    private Transform coreTarget;
    private Transform currentTarget;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform;

        // Requiere que le asignes el tag "Core" al objeto de tu núcleo en el Inspector
        GameObject coreObj = GameObject.FindGameObjectWithTag("Core");
        if (coreObj != null)
        {
            coreTarget = coreObj.transform;
        }

        // El objetivo primario al instanciarse es el núcleo
        currentTarget = coreTarget;
    }

    private void Update()
    {
        if (currentTarget == null) return;

        distanceToTarget = Vector3.Distance(currentTarget.position, transform.position);

        if (isProvoked)
        {
            EngageTarget();

            // --- NUEVO: Límite de persecución ---
            // Si está persiguiendo al jugador y este se aleja demasiado (ej. el doble del chaseRange)
            if (currentTarget == playerTarget && distanceToTarget > chaseRange * 15f)
            {
                isProvoked = false;
                currentTarget = coreTarget; // Vuelve a enfocar el Núcleo
                Debug.Log("El enemigo ha perdido interés en Aris y regresa al núcleo.");
            }
        }
        else if (distanceToTarget <= chaseRange)
        {
            isProvoked = true;
        }
    }

    public void OnDamageTaken()
    {
        // Transición de objetivo al recibir un disparo
        currentTarget = playerTarget;
        isProvoked = true;
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
        // Discriminación estructural para evitar NullReferenceException
        if (currentTarget.CompareTag("Player"))
        {
            PlayerBehaviour playerHealth = currentTarget.GetComponent<PlayerBehaviour>();
            if (playerHealth != null) playerHealth.playerHealth -= damage;
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
}