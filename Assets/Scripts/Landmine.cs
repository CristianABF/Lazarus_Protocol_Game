using UnityEngine;

public abstract class Landmine : MonoBehaviour
{
    [Header("Configuración Base")]
    [SerializeField] protected float triggerRadius = 2.5f;
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] protected GameObject explosionEffect;
    [SerializeField] protected AudioClip soundEffect;

    protected bool isTriggered = false;

    protected virtual void Awake()
    {
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = triggerRadius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && ((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            isTriggered = true;
            OnDetonate(other.gameObject);
        }
    }

    protected abstract void OnDetonate(GameObject target);
}
