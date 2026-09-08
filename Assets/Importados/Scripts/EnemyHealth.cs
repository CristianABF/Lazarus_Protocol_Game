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
            // you are dead
            // "play death animation"
            Destroy(gameObject, 4f);
            isDead = true;
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
    }
}
