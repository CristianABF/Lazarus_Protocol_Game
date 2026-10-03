using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 50f;
    [SerializeField] private float lifeTime = 3f; // Tiempo antes de autodestruirse si no impacta

    private float damage;

    private void Start()
    {
        // Destruir la bala despues de 'lifeTime' segundos si no choca con nada
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Mover la bala hacia adelante
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // Configurar el daño que infligira esta instancia
    public void Setup(float damageAmount)
    {
        damage = damageAmount;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignorar colisiones con otras balas o desencadenantes que no sean solidos
        if (other.isTrigger) return;

        // Comprobacion por Tag "Enemy" o si el objetivo pertenece a un enemigo
        if (other.CompareTag("Enemy") || other.transform.root.CompareTag("Enemy"))
        {
            Component damageable = other.transform.GetComponentInParent(typeof(IDamageable));
            if (damageable != null)
            {
                GameFunctions.Attack(damageable, damage);
            }

            // Notificar a la IA del enemigo
            EnemyAI enemy = other.transform.GetComponentInParent<EnemyAI>();
            if (enemy != null) enemy.OnDamageTaken();
        }

        // Destruir la bala al impactar contra cualquier superficie solida (Enemigo, Pared, Suelo)
        Destroy(gameObject);
    }
}
