using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class BarricadeObstacle : MonoBehaviour
{
    private NavMeshObstacle navObstacle;

    private void Awake()
    {
        navObstacle = GetComponent<NavMeshObstacle>();
    }

    public void OnPlaced()
    {
        // Activar el carving para recortar el NavMesh y forzar a los enemigos a recalcular ruta
        navObstacle.enabled = true;

        // Opcional: Notificar a los agentes cercanos para que recalculen su ruta de inmediato
        RecalculateNearbyEnemies();
    }

    private void RecalculateNearbyEnemies()
    {
        // Busca enemigos en un área cercana y actualiza su destino
        Collider[] hits = Physics.OverlapSphere(transform.position, 15f);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
            {
                if (agent.hasPath)
                {
                    agent.SetDestination(agent.destination); // Setea de nuevo el destino para forzar la reevaluación
                }
            }
        }
    }
}