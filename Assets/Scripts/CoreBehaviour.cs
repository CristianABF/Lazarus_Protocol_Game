using UnityEngine;

public class CoreBehaviour : MonoBehaviour
{
    [Header("Integridad de la Oleada (Temporal)")]
    public float maxHealth = 500f; // Mucha más vida que el jugador
    public float currentHealth;
    private bool isDestroyed = false;

    // Aquí a futuro deberás inyectar la variable de "Salud Permanente" 
    // que se degrada entre partidas.

    private void Start()
    {
        currentHealth = maxHealth;
    }

    // Método blindado que llamarán los enemigos
    public void RecibirDaño(float amount)
    {
        if (isDestroyed) return;

        currentHealth -= amount;
        Debug.Log($"[NÚCLEO] Daño recibido: {amount}. Integridad restante: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            ColapsoInminente();
        }
    }

    private void ColapsoInminente()
    {
        isDestroyed = true;
        currentHealth = 0;

        Debug.LogWarning("¡EL NÚCLEO HA COLAPSADO!");

        // --- DELEGACIÓN AL GAMEMANAGER ---
        // Aquí NO debes programar menús. Debes llamar al cerebro de tu juego:
        // GameManager.Instancia.EjecutarProtocoloLazaro();
    }
}