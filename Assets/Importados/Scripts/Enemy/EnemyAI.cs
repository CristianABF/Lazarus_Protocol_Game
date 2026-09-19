using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float distance = 2.5f;

    private NavMeshAgent navMeshAgent;
    private float distanceToTarget = Mathf.Infinity;
    private bool isProvoked = false;
    private Transform target;
    private float damage = 10f;

    [Header("Attack Speed")]
    private float attackRate = 2f;
    private float lastAttack = 0f;

    [Header("Reload")]
    private float reloadSpeed = 3.3f;
    private int ammoAmount = 5;
    private int reloadAmount = 5;
    private bool isReloading = false;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        distanceToTarget = Vector3.Distance(target.position, transform.position);

        if (isProvoked)
        {
            EngageTarget();
        }
        else if (distanceToTarget <= chaseRange)
        {
            isProvoked = false;
        }
    }

    public void OnDamageTaken()
    {
        isProvoked = true;
    }

    private void EngageTarget()
    {
        FaceTarget();

        if (!isReloading)
        {
            if (distanceToTarget > distance)
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
                target.GetComponent<PlayerBehaviour>().playerHealth -= damage;
                GetComponentInChildren<Animator>().SetTrigger("shoot");
                ammoAmount--;
            }
            else if (ammoAmount <= 0)
            {
                Wait();
                Reload();
            }
            else
            {
                ChaseTarget();
            }
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
        navMeshAgent.SetDestination(target.position);
    }

    private void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;

        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}
