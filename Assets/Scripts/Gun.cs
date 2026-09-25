using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour, IPickable
{
    [Header("Sincronizacion del Cargador")]
    [SerializeField] private GameObject gunMagazine;
    [SerializeField] private GameObject handMagazine;

    [Header("Configuración del Arma")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private bool isAutomatic;
    [SerializeField] private float range = 100f;

    [SerializeField] private Transform cameraRoot;
    public Camera fpsCam;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Animator playerAnimator;

    [SerializeField] private float laserDuration = 0.05f;

    private float attackTime;
    private float recoilSpeed = 0f;
    private bool isFiring;
    private AudioSource gunSound;
    private Ammo ammo;
    private LineRenderer laserLine;

    private void Awake()
    {
        gunSound = GetComponent<AudioSource>();
        ammo = GetComponent<Ammo>();
        laserLine = GetComponent<LineRenderer>();
        laserLine.enabled = false;
    }

    // metodos de la interfaz
    public void OnPickedUp()
    {
        this.enabled = true;
        if (ammo != null) ammo.enabled = true;
    }

    public void OnDropped()
    {
        this.enabled = false;
        if (ammo != null) ammo.enabled = false;
    }

    private void OnEnable()
    {
        InputController.Input.Player.Shoot.performed += Shoot;
        InputController.Input.Player.Shoot.canceled += Shoot_canceled;
    }

    private void OnDisable()
    {
        if (InputController.Input != null)
        {
            InputController.Input.Player.Shoot.performed -= Shoot;
            InputController.Input.Player.Shoot.canceled -= Shoot_canceled;
        }
    }

    private void Update()
    {
        if (isFiring && ammo.GetCurrentAmmo() > 0)
        {
            attackTime += Time.deltaTime;

            if (attackTime >= fireRate)
            {
                attackTime = 0f;
                IsShooting();
            }
        }
        else
        {
            isFiring = false;
        }
    }

    private void IsShooting()
    {
        if (ammo != null) ammo.ReduceCurrentAmmo();
        if (cameraRoot != null) Recoil();
        if (gunSound != null) gunSound.Play();

        if (playerAnimator != null) playerAnimator.SetTrigger("Shoot");

        if (fpsCam == null)
        {
            Debug.LogError("¡Asigna la fpsCam en el Inspector!");
            return;
        }
        // inicia el destello visual
        StartCoroutine(ShootLaser());
        Vector3 rayOrigin = fpsCam.transform.position;
        Vector3 rayDirection = fpsCam.transform.forward;

        // punto de inicio del rayo
        laserLine.SetPosition(0, firePoint != null ? firePoint.position : transform.position);

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, range))
        {
            // si impacta, el final del rayo es el punto de impacto exacto
            laserLine.SetPosition(1, hit.point);

            if (hit.transform.CompareTag("Enemy"))
            {
                Component damageable = hit.transform.GetComponent(typeof(IDamageable));
                if (damageable) GameFunctions.Attack(damageable, damage);

                ParticleSystem ps = hit.transform.GetComponentInChildren<ParticleSystem>();
                if (ps != null) ps.Play();

                EnemyAI enemy = hit.transform.GetComponent<EnemyAI>();
                if (enemy != null) enemy.OnDamageTaken();
            }
        }
        else
        {
            // si no impacta nada, el rayo se dibuja hasta el límite del rango
            laserLine.SetPosition(1, rayOrigin + (rayDirection * range));
        }
    }

    private IEnumerator ShootLaser()
    {
        laserLine.enabled = true;
        float timer = 0f;

        while (timer < laserDuration)
        {
            // mantiene el punto 0 pegado al cañon del arma en cada frame
            laserLine.SetPosition(0, firePoint != null ? firePoint.position : transform.position);

            timer += Time.deltaTime;
            yield return null; // espera al siguiente frame antes de repetir
        }
        laserLine.enabled = false;
    }

    private void Shoot(InputAction.CallbackContext context)
    {
        if (PauseControl.isPaused) return;

        if (isAutomatic && ammo.GetCurrentAmmo() > 0)
        {
            isFiring = true;
        }
        else if (!isAutomatic && ammo.GetCurrentAmmo() > 0)
        {
            IsShooting();
        }
    }

    private void Shoot_canceled(InputAction.CallbackContext obj)
    {
        isFiring = false;
    }

    private void Recoil()
    {
        cameraRoot.Rotate(recoilSpeed, 0, 0);
    }

    public void Animation_DetachMagazine()
    {
        if (gunMagazine != null) gunMagazine.SetActive(false);
        if (handMagazine != null) handMagazine.SetActive(true);
    }

    public void Animation_AttachMagazine()
    {
        // Regresa el cargador al arma y oculta el de la mano
        if (gunMagazine != null) gunMagazine.SetActive(true);
        if (handMagazine != null) handMagazine.SetActive(false);
    }

    public void Animation_FinishReload()
    {
        if (ammo != null) ammo.ExecuteReload();
    }
}
