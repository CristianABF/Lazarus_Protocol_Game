using System;
using UnityEngine;

public class BuildableObject : MonoBehaviour
{
    public event Action<BuildableObject> OnObjectDestroyed;

    private void OnDestroy()
    {
        OnObjectDestroyed?.Invoke(this);
    }
}