using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public float health;
    public float maxHealth = 50f;
    private bool isDead = false;

    [Header("Botín (Drop)")]
    public GameObject chatarraPrefab;
    public int cantidadChatarra;

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

        // Liberar cupo en el núcleo inmediatamente al morir
        GameObject coreObj = GameObject.FindGameObjectWithTag("Core");
        if (coreObj != null)
        {
            CoreBehaviour core = coreObj.GetComponent<CoreBehaviour>();
            if (core != null)
            {
                core.LiberarLugar(transform);
            }
        }

        // Desactiva los componentes tácticos
        EnemyAI aiScript = GetComponent<EnemyAI>();
        if (aiScript != null) aiScript.enabled = false;

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // --- CONEXIÓN A LAS ESTADÍSTICAS DEL GAME OVER ---
        if (ControladorPartida.Instancia != null)
        {
            ControladorPartida.Instancia.RegistrarEnemigoDerrotado();
        }

        // --- LÓGICA DE SOLTAR CHATARRA ---
        if (chatarraPrefab != null && cantidadChatarra > 0)
        {
            for (int i = 0; i < cantidadChatarra; i++)
            {
                Vector3 variacionPosicion = new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, Random.Range(-0.5f, 0.5f));
                Instantiate(chatarraPrefab, transform.position + variacionPosicion, Quaternion.identity);
            }
        }

        Destroy(gameObject, 0.1f);
    }
}