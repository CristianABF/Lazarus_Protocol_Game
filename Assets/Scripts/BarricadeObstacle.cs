using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class BarricadeObstacle : MonoBehaviour
{
    [SerializeField] private float lifetime = 10f; // Tiempo en segundos antes de destruirse
    private NavMeshObstacle navObstacle;

    // Evento para notificar la destrucción
    public event Action<BarricadeObstacle> OnDestroyed;

    private void Awake()
    {
        navObstacle = GetComponent<NavMeshObstacle>();
    }

    public void OnPlaced()
    {
        navObstacle.enabled = true;
        RecalculateNearbyEnemies();

        // Destruye el GameObject automáticamente al pasar el tiempo de vida
        Destroy(gameObject, lifetime);
    }

    private void OnDestroy()
    {
        // Notifica a quien esté escuchando que este objeto se eliminó
        OnDestroyed?.Invoke(this);
    }

    private void RecalculateNearbyEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 15f);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
            {
                if (agent.hasPath)
                {
                    agent.SetDestination(agent.destination);
                }
            }
        }
    }
}