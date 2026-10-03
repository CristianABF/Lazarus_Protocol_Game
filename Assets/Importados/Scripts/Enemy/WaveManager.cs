using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [Header("Elementos de UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI phaseText;

    [Header("Configuración de Enemigos")]
    [SerializeField] private GameObject[] enemyPrefabs; // Ahora es una lista para múltiples enemigos
    [SerializeField] private Transform[] spawnPoints;
    public int enemiesPerWave = 3;

    private float timeElapsed = 0f;
    private int currentPhaseIndex = -1;

    [System.Serializable]
    public struct GamePhase
    {
        public string phaseName;
        public float startTimeInSeconds;
        public bool isWave;
    }

    [Header("Configuración de Fases")]
    public List<GamePhase> gamePhases;

    void Start()
    {
        if (gamePhases.Count == 0)
        {
            gamePhases = new List<GamePhase>
            {
                new GamePhase { phaseName = "Preparación inicial", startTimeInSeconds = 0f, isWave = false },
                new GamePhase { phaseName = "Oleada 1 (Brecha inicial)", startTimeInSeconds = 60f, isWave = true },
                new GamePhase { phaseName = "Descanso de reabastecimiento", startTimeInSeconds = 150f, isWave = false },
                new GamePhase { phaseName = "Oleada 2 (Asedio estándar)", startTimeInSeconds = 195f, isWave = true },
                new GamePhase { phaseName = "Descanso táctico", startTimeInSeconds = 345f, isWave = false },
                new GamePhase { phaseName = "Oleada 3 (Escalada pesada)", startTimeInSeconds = 375f, isWave = true },
                new GamePhase { phaseName = "Descanso asfixiante", startTimeInSeconds = 555f, isWave = false },
                new GamePhase { phaseName = "Oleada 4 (Colapso parcial)", startTimeInSeconds = 575f, isWave = true },
                new GamePhase { phaseName = "Descanso crítico", startTimeInSeconds = 800f, isWave = false },
                new GamePhase { phaseName = "Oleada 5 (Supervivencia final)", startTimeInSeconds = 815f, isWave = true }
            };
        }
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        UpdateTimerUI();
        CheckPhases();
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timeElapsed / 60);
        int seconds = Mathf.FloorToInt(timeElapsed % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void CheckPhases()
    {
        if (currentPhaseIndex + 1 < gamePhases.Count)
        {
            if (timeElapsed >= gamePhases[currentPhaseIndex + 1].startTimeInSeconds)
            {
                currentPhaseIndex++;
                StartCoroutine(ShowPhaseAnnouncement(gamePhases[currentPhaseIndex].phaseName));

                if (gamePhases[currentPhaseIndex].isWave)
                {
                    SpawnEnemies();
                }
            }
        }
    }

    void SpawnEnemies()
    {
        // Verifica que haya al menos un enemigo y un punto de aparición cargados
        if (enemyPrefabs.Length > 0 && spawnPoints.Length > 0)
        {
            for (int i = 0; i < enemiesPerWave; i++)
            {
                // Elige un enemigo al azar de tu lista
                int randomEnemyIndex = Random.Range(0, enemyPrefabs.Length);
                GameObject selectedEnemy = enemyPrefabs[randomEnemyIndex];

                // Elige un punto de aparición al azar de tu lista
                int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
                Transform spawnPoint = spawnPoints[randomSpawnIndex];

                Instantiate(selectedEnemy, spawnPoint.position, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("Faltan asignar los Enemigos o los Spawn Points en el GameManager.");
        }
    }

    IEnumerator ShowPhaseAnnouncement(string message)
    {
        phaseText.text = "¡" + message + "!";
        yield return new WaitForSeconds(4f);
        phaseText.text = "";
    }
}
