using UnityEngine;

public class DebugDestruccion : MonoBehaviour
{
    private void OnDestroy()
    {
        Debug.LogError("¡Me destruyeron!");
    }
}