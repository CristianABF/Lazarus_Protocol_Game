using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 50f;
    [SerializeField] private float lifeTime = 3f;

    private float damage;
    private Transform owner; // Quién disparó la bala

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // Acepta opcionalmente quién disparó la bala
    public void Setup(float damageAmount, Transform bulletOwner = null)
    {
        damage = damageAmount;
        owner = bulletOwner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        if (other.CompareTag("Enemy") || other.transform.root.CompareTag("Enemy"))
        {
            Component damageable = other.transform.GetComponentInParent(typeof(IDamageable));
            if (damageable != null)
            {
                GameFunctions.Attack(damageable, damage);
            }

            // Notifica a la IA pasando quién fue el dueño del disparo
            EnemyAI enemy = other.transform.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                enemy.OnDamageTaken(owner);
            }
        }

        Destroy(gameObject);
    }
}