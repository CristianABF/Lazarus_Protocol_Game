using System.Collections.Generic;
using UnityEngine;

public class CoreBehaviour : MonoBehaviour
{
    [Header("Integridad de la Oleada")]
    public float maxHealth = 2500f;
    public float currentHealth = 2500f;
    private bool isDestroyed = false;

    [Header("Configuración de Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audio75Percent;
    [SerializeField] private AudioClip audio50Percent;
    [SerializeField] private AudioClip audio25Percent;

    [Header("Configuración de Ataque")]
    [SerializeField] private int maxAttackers = 3;
    private List<Transform> currentAttackers = new List<Transform>();

    private bool played75 = false;
    private bool played50 = false;
    private bool played25 = false;

    private void Start()
    {
        currentHealth = maxHealth;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public bool SolicitarnLugarParaAtacar(Transform enemigo)
    {
        currentAttackers.RemoveAll(a => a == null);

        if (currentAttackers.Contains(enemigo)) return true;

        if (currentAttackers.Count < maxAttackers)
        {
            currentAttackers.Add(enemigo);
            return true;
        }

        return false;
    }

    public void LiberarLugar(Transform enemigo)
    {
        if (currentAttackers.Contains(enemigo))
        {
            currentAttackers.Remove(enemigo);
        }
    }

    public void RecibirDaño(float amount)
    {
        if (isDestroyed) return;

        currentHealth -= amount;

        // Feedback por consola
        Debug.Log($"[NÚCLEO] Impacto enemigo. Integridad restante: {currentHealth}/{maxHealth}");

        // Evaluar umbrales de salud para reproducir los audios
        VerificarUmbralesSalud();

        if (currentHealth <= 0)
        {
            ColapsoInminente();
        }
    }

    private void VerificarUmbralesSalud()
    {
        float porcentajeSalud = (currentHealth / maxHealth) * 100f;

        // Umbral del 75%
        if (porcentajeSalud <= 75f && !played75)
        {
            played75 = true;
            ReproducirAudio(audio75Percent);
        }
        // Umbral del 50%
        else if (porcentajeSalud <= 50f && !played50)
        {
            played50 = true;
            ReproducirAudio(audio50Percent);
        }
        // Umbral del 25%
        else if (porcentajeSalud <= 25f && !played25)
        {
            played25 = true;
            ReproducirAudio(audio25Percent);
        }
    }

    private void ReproducirAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            // PlayOneShot permite reproducir el efecto sin cortar audios anteriores
            audioSource.PlayOneShot(clip);
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