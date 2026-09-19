using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public float health;
    public float maxHealth;
    private bool isDead = false;

    private void Awake()
    {
        health = maxHealth;
    }

    private void Update()
    {
        if (health <= 0 && !isDead)
        {
            isDead = true;
            GetComponent<EnemyAI>().enabled = false;
            GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
            // para activar animacion de muerte cuando haya una
            //GetComponentInChildren<Animator>().SetTrigger("die");
            Destroy(gameObject, 0f);
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
    }
}
