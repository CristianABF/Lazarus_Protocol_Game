using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class WaveManager : MonoBehaviour
{
    public static event Action<int> OnPhaseStarted;

    [Header("Elementos de UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI phaseText;

    [Header("Configuración de Enemigos")]
    [SerializeField] private GameObject[] enemyPrefabs;
    public GameObject[] GetEnemyPrefabs() => enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    public int enemiesPerWave = 5;

    private float timeElapsed = 0f;
    private int currentPhaseIndex = -1;
    private float phaseTimer = 0f;
    private float spawnTimer = 0f; // Temporizador para cada enemigo
    private bool isWaveActive = false;

    [System.Serializable]
    public struct GamePhase
    {
        public string phaseName;
        public float phaseDuration;
        public float spawnInterval; // Tiempo en segundos entre cada enemigo
    }

    [Header("Configuración de Fases")]
    [SerializeField] private GamePhase[] gamePhases;

    void Start()
    {
        if (phaseText != null)
        {
            phaseText.gameObject.SetActive(false);
        }

        // Solo arranca desde la fase 0 si no se restauró una partida guardada antes
        if (gamePhases.Length > 0 && currentPhaseIndex == -1)
        {
            StartNextPhase();
        }
    }

    public float GetTiempoTranscurrido()
    {
        return timeElapsed;
    }

    public void RestaurarTiempoYFase(float segundosGuardados)
    {
        timeElapsed = segundosGuardados;
        UpdateTimerUI();

        float tiempoAcumulado = 0f;
        for (int i = 0; i < gamePhases.Length; i++)
        {
            if (segundosGuardados < tiempoAcumulado + gamePhases[i].phaseDuration)
            {
                currentPhaseIndex = i;
                phaseTimer = (tiempoAcumulado + gamePhases[i].phaseDuration) - segundosGuardados;
                spawnTimer = 0f;
                isWaveActive = true;
                GuardarManager.enDescanso = (gamePhases[i].spawnInterval <= 0f);
                return;
            }
            tiempoAcumulado += gamePhases[i].phaseDuration;
        }
    }

    void Update()
    {
        // 1. Temporizador global
        timeElapsed += Time.deltaTime;
        UpdateTimerUI();

        // 2. Lógica de las fases
        if (isWaveActive && gamePhases.Length > 0)
        {
            phaseTimer -= Time.deltaTime;

            GamePhase currentPhase = gamePhases[currentPhaseIndex];

            // Generación continua de enemigos
            if (currentPhase.spawnInterval > 0)
            {
                spawnTimer -= Time.deltaTime;
                if (spawnTimer <= 0)
                {
                    SpawnEnemies(1); // Genera 1 solo enemigo
                    spawnTimer = currentPhase.spawnInterval; // Reinicia el reloj
                }
            }

            // Cambiar de fase
            if (phaseTimer <= 0)
            {
                StartNextPhase();
            }
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeElapsed / 60F);
            int seconds = Mathf.FloorToInt(timeElapsed % 60F);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void StartNextPhase()
    {
        currentPhaseIndex++;

        if (currentPhaseIndex < gamePhases.Length)
        {
            GamePhase currentPhase = gamePhases[currentPhaseIndex];
            phaseTimer = currentPhase.phaseDuration;
            spawnTimer = 0f; // Para que el primer enemigo salga de inmediato
            isWaveActive = true;

            // Si spawnInterval es 0 (o menor), es fase de descanso y permite guardar.
            // Si es mayor a 0, hay oleada activa y bloquea el guardado.
            GuardarManager.enDescanso = (currentPhase.spawnInterval <= 0f);

            OnPhaseStarted?.Invoke(currentPhaseIndex + 1);
            StartCoroutine(ShowPhaseMessage(currentPhase.phaseName, 3f));
        }
        else
        {
            isWaveActive = false;
            GuardarManager.enDescanso = false; // Ya terminó la partida
            StartCoroutine(ShowPhaseMessage("¡Supervivencia Completada!", 5f));
        }
    }

    void SpawnEnemies(int count)
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            GameObject randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(randomEnemy, randomSpawn.position, randomSpawn.rotation);
        }
    }

    IEnumerator ShowPhaseMessage(string message, float displayTime)
    {
        if (phaseText != null)
        {
            phaseText.text = message;
            phaseText.gameObject.SetActive(true);
            yield return new WaitForSeconds(displayTime);
            phaseText.gameObject.SetActive(false);
        }
    }
}