using UnityEngine;

public class BuildingUnlockManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private WrenchBuilder wrenchBuilder;

    [System.Serializable]
    public class PhaseUnlockCondition
    {
        [Tooltip("ID único configurado en WrenchBuilder (ej: ElectricMine)")]
        public string itemIdToUnlock;
        [Tooltip("Ronda/Fase necesaria para desbloquear el plano")]
        public int requiredPhase = 2;
        [HideInInspector] public bool isUnlocked = false;
    }

    [System.Serializable]
    public class KillsUpgradeCondition
    {
        [Tooltip("ID único configurado en WrenchBuilder (ej: Barricade)")]
        public string itemIdToUpgrade;
        [Tooltip("Bajas totales necesarias en ControladorPartida")]
        public int requiredKills;
        [Tooltip("Cantidad de unidades extra que suma al límite actual")]
        public int extraMaxAmount = 1;
        [Tooltip("Límite MÁXIMO ABSOLUTO que no se podrá superar nunca con kills")]
        public int absoluteMaxLimit = 10;
        [HideInInspector] public bool isApplied = false;
    }

    [Header("1. Desbloqueo de Planos por Oleada")]
    [SerializeField] private PhaseUnlockCondition[] phaseUnlockConditions;

    [Header("2. Aumento de Límite por Bajas de Enemigos")]
    [SerializeField] private KillsUpgradeCondition[] killsUpgradeConditions;

    private void OnEnable()
    {
        WaveManager.OnPhaseStarted += EvaluatePhaseUnlocks;
        ControladorPartida.OnEnemyKilled += EvaluateKillsUpgrades;
    }

    private void OnDisable()
    {
        WaveManager.OnPhaseStarted -= EvaluatePhaseUnlocks;
        ControladorPartida.OnEnemyKilled -= EvaluateKillsUpgrades;
    }

    private void EvaluatePhaseUnlocks(int currentPhase)
    {
        foreach (var condition in phaseUnlockConditions)
        {
            if (!condition.isUnlocked && currentPhase >= condition.requiredPhase)
            {
                condition.isUnlocked = true;

                if (wrenchBuilder != null)
                {
                    wrenchBuilder.UnlockItem(condition.itemIdToUnlock);
                }
            }
        }
    }

    private void EvaluateKillsUpgrades(int currentKills)
    {
        foreach (var condition in killsUpgradeConditions)
        {
            if (!condition.isApplied && currentKills >= condition.requiredKills)
            {
                condition.isApplied = true;

                if (wrenchBuilder != null)
                {
                    wrenchBuilder.IncreaseMaxAmount(condition.itemIdToUpgrade, condition.extraMaxAmount, condition.absoluteMaxLimit);
                }
            }
        }
    }
}