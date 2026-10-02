using UnityEngine;

public class FlamethrowerTurret : TurretBase
{
    [Header("Parametros Lanzallamas")]
    [SerializeField] private float continuousDamagePerSecond = 20f;
    [SerializeField] private ParticleSystem flameParticles;

    protected override void Update()
    {
        base.Update();

        // Control visual de las particulas de fuego
        if (target != null && flameParticles != null)
        {
            if (!flameParticles.isPlaying) flameParticles.Play();
        }
        else if (flameParticles != null && flameParticles.isPlaying)
        {
            flameParticles.Stop();
        }
    }

    protected override void Attack()
    {
        // Para daño continuom, calculamos el daño multiplicando por Time.deltaTime
        if (target != null)
        {
            if (target.TryGetComponent(out IDamageable enemy))
            {
                enemy.TakeDamage(continuousDamagePerSecond * Time.deltaTime);
            }
        }
    }
}
