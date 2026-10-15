using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public float maxHealth = 100f;
    public float playerHealth;
    private bool isDead = false;

    private void Awake()
    {
        playerHealth = maxHealth;
    }

//    private void Update()
//    {
//        PlayerDead();
//    }

    private void PlayerDead()
    {
        // Añadimos la comprobación !isDead para evitar que la función se llame 60 veces por segundo si la vida sigue en 0
        if (playerHealth <= 0f && !isDead)
        {
            isDead = true;
            Debug.LogWarning("Aris ha caído en combate.");

            // Le avisamos al árbitro global que la oleada fracasó por muerte del jugador
            if (ControladorPartida.Instancia != null)
            {
                ControladorPartida.Instancia.ProcesarDerrotaOleada();
            }
        }
    }

    public void ResetState() { isDead = false; }
    public bool GetState() { return isDead; }

    public void RecibirDano(float cantidad)
    {
        if (isDead) return;

        playerHealth -= cantidad;

        // 1. Notificamos inmediatamente al cerebro global para las estadísticas
        if (ControladorPartida.Instancia != null)
        {
            ControladorPartida.Instancia.RegistrarDanoRecibido(cantidad);
        }

        // 2. Evaluamos la muerte en el milisegundo exacto del impacto, no en el Update
        if (playerHealth <= 0f)
        {
            PlayerDead();
        }
    }

    public void RecibirCuracion(float cantidad)
    {
        if (isDead) return; // Evita curar al jugador en el frame que muere

        playerHealth += cantidad;

        // El tope estricto
        if (playerHealth > maxHealth)
        {
            playerHealth = maxHealth;
        }
    }
}