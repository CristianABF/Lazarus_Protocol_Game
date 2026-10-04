using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Necesario para TextMeshPro

public class WaveManager : MonoBehaviour
{
    [Header("Elementos de UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI phaseText;

    [Header("Configuración de Enemigos")]
    [SerializeField] private GameObject[] enemyPrefabs; // Lista para múltiples enemigos
    [SerializeField] private Transform[] spawnPoints;
    public int enemiesPerWave = 5;

    // Variables internas de tiempo y fases
    private float timeElapsed = 0f;
    private int currentPhaseIndex = -1;
    private float phaseTimer = 0f;
    private bool isWaveActive = false;

    [System.Serializable]
    public struct GamePhase
    {
        public string phaseName;       // Ej: "Oleada 1", "¡Sobrevive!"
        public float phaseDuration;    // Duración de la fase en segundos
        public int enemiesToSpawn;     // Enemigos a generar en esta fase
    }

    [Header("Configuración de Fases")]
    [SerializeField] private GamePhase[] gamePhases;

    void Start()
    {
        // Limpiamos el texto del medio al iniciar
        if (phaseText != null)
        {
            phaseText.gameObject.SetActive(false);
        }

        // Si configuraste fases en el Inspector, iniciamos la primera
        if (gamePhases.Length > 0)
        {
            StartNextPhase();
        }
    }

    void Update()
    {
        // 1. Temporizador global que cuenta hacia adelante
        timeElapsed += Time.deltaTime;
        UpdateTimerUI();

        // 2. Lógica de las fases
        if (isWaveActive && gamePhases.Length > 0)
        {
            phaseTimer -= Time.deltaTime;

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
            // Calcula minutos y segundos
            int minutes = Mathf.FloorToInt(timeElapsed / 60F);
            int seconds = Mathf.FloorToInt(timeElapsed % 60F);

            // Formatea el texto a "00:00"
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void StartNextPhase()
    {
        currentPhaseIndex++;

        // Verificamos que queden fases disponibles
        if (currentPhaseIndex < gamePhases.Length)
        {
            GamePhase currentPhase = gamePhases[currentPhaseIndex];
            phaseTimer = currentPhase.phaseDuration;
            isWaveActive = true;

            // Mostramos el aviso en el centro durante 3 segundos
            StartCoroutine(ShowPhaseMessage(currentPhase.phaseName, 3f));

            // Generamos los enemigos de esta oleada
            SpawnEnemies(currentPhase.enemiesToSpawn);
        }
        else
        {
            // Ya no hay más fases
            isWaveActive = false;
            StartCoroutine(ShowPhaseMessage("¡Supervivencia Completada!", 5f));
        }
    }

    void SpawnEnemies(int count)
    {
        // Verificamos que haya enemigos y puntos de spawn configurados
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            // Elige un enemigo y un punto de spawn al azar
            GameObject randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

            Instantiate(randomEnemy, randomSpawn.position, randomSpawn.rotation);
        }
    }

    // Corrutina para mostrar y luego ocultar el texto central
    IEnumerator ShowPhaseMessage(string message, float displayTime)
    {
        if (phaseText != null)
        {
            phaseText.text = message;
            phaseText.gameObject.SetActive(true); // Activa el texto

            yield return new WaitForSeconds(displayTime); // Espera los segundos indicados

            phaseText.gameObject.SetActive(false); // Apaga el texto
        }
    }
}