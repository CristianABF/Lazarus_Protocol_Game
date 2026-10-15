using UnityEngine;

public class CoreBehaviour : MonoBehaviour
{
    [Header("Integridad de la Oleada (Temporal)")]
    public float maxHealth = 2500f;
    public float currentHealth = 2500f;
    private bool isDestroyed = false;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void RecibirDaño(float amount)
    {
        if (isDestroyed) return;

        currentHealth -= amount;

        // Retroalimentación temporal por consola
        Debug.Log($"[NÚCLEO] Impacto enemigo. Integridad restante: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            ColapsoInminente();
        }
    }

    private void ColapsoInminente()
    {
        isDestroyed = true;
        currentHealth = 0;

        Debug.LogWarning("¡ALERTA CRÍTICA: EL NÚCLEO HA COLAPSADO!");

        // Delegación al ControladorPartida
        if (ControladorPartida.Instancia != null)
        {
            ControladorPartida.Instancia.ProcesarDerrotaOleada();
        }
    }
}