using UnityEngine;

public class FlamethrowerTurret : TurretBase
{
    [Header("Parámetros Lanzallamas")]
    [SerializeField] private float continuousDamagePerSecond = 20f;
    [SerializeField] private ParticleSystem flameParticles;

    protected override void Update()
    {
        base.Update();

        // Control visual de partículas de fuego según la presencia del objetivo
        if (target != null)
        {
            if (flameParticles != null && !flameParticles.isPlaying)
            {
                flameParticles.Play();
            }
        }
        else
        {
            if (flameParticles != null && flameParticles.isPlaying)
            {
                flameParticles.Stop();
            }
        }
    }

    protected override void Attack()
    {
        if (target == null) return;

        if (target.CompareTag("Enemy") || target.root.CompareTag("Enemy"))
        {
            Component damageable = target.GetComponentInParent(typeof(IDamageable));
            if (damageable != null)
            {
                GameFunctions.Attack(damageable, continuousDamagePerSecond * Time.deltaTime);
            }

            EnemyAI enemy = target.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                enemy.OnDamageTaken();
            }
        }
    }
}