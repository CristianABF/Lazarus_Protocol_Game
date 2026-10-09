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
    [SerializeField] private LayerMask layerMaskDisparo; // Capas con las que SI puede colisionar

    [Header("Proyectil")]
    [SerializeField] private GameObject bulletPrefab; // Prefab de la bala que tiene el script Bullet.cs

    [SerializeField] private Transform cameraRoot;
    public Camera fpsCam;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Animator playerAnimator;

    private float attackTime;
    private float recoilSpeed = 0f;
    private bool isFiring;
    private AudioSource gunSound;
    private Ammo ammo;

    private void Awake()
    {
        gunSound = GetComponent<AudioSource>();
        ammo = GetComponent<Ammo>();
    }

    // Métodos de la interfaz IPickable
    public void OnPickedUp()
    {
        this.enabled = true;
        if (ammo != null) ammo.enabled = true;

        if (fpsCam == null)
        {
            fpsCam = Camera.main;
        }
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
            fpsCam = Camera.main;
            if (fpsCam == null)
            {
                Debug.LogError("¡No se encontró 'fpsCam' ni 'Camera.main'!");
                return;
            }
        }

        if (bulletPrefab == null)
        {
            Debug.LogError("¡Asigna el bulletPrefab en el Inspector de Gun!");
            return;
        }

        // 1. Calcular el centro exacto de la pantalla de la cámara
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;

        // Usamos QueryTriggerInteraction.Ignore para ignorar triggers y evitar colisionar con el jugador
        if (Physics.Raycast(ray, out RaycastHit hit, range, layerMaskDisparo, QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(range);
        }

        // 2. Punto de origen de la bala
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;

        // 3. Dirección real del disparo hacia el objetivo
        Vector3 direction = (targetPoint - spawnPosition).normalized;
        Quaternion bulletRotation = Quaternion.LookRotation(direction);

        // 4. Instanciar la bala y configurar daño
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPosition, bulletRotation);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.Setup(damage);
        }
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

    public void Reload()
    {
        if (ammo != null)
        {
            ammo.ExecuteReload();
        }
    }

    public void Animation_DetachMagazine()
    {
        if (gunMagazine != null) gunMagazine.SetActive(false);
        if (handMagazine != null) handMagazine.SetActive(true);
    }

    public void Animation_AttachMagazine()
    {
        if (gunMagazine != null) gunMagazine.SetActive(true);
        if (handMagazine != null) handMagazine.SetActive(false);
    }

    public void Animation_FinishReload()
    {
        if (ammo != null) ammo.ExecuteReload();
    }
}