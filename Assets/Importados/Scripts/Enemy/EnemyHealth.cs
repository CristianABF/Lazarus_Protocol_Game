using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public float health;
    public float maxHealth = 50f;
    private bool isDead = false;

    private void Awake()
    {
        health = maxHealth;
    }

    // Se elimina el Update() por completo para optimizar el rendimiento del motor

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;

        // Puente interno: Le avisamos al script de IA que nos dispararon para activar el agro
        EnemyAI ia = GetComponent<EnemyAI>();
        if (ia != null) ia.OnDamageTaken();

        if (health <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        isDead = true;

        // Desactiva los componentes tácticos
        GetComponent<EnemyAI>().enabled = false;
        GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;

        // --- CONEXIÓN A LAS ESTADÍSTICAS DEL GAME OVER ---
        if (ControladorPartida.Instancia != null)
        {
            ControladorPartida.Instancia.RegistrarEnemigoDerrotado();
        }

        // Se destruye el objeto (dejamos 0.1f para evitar errores de colisión en ese mismo frame)
        Destroy(gameObject, 0.1f);
    }
}