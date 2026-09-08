using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    [SerializeField] private float damage = 20f;
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private bool isAutomatic;
    [SerializeField] private Transform cameraRoot;
    public Camera fpsCam;

    private float range = 100f;
    private float attackTime;
    private float recoilSpeed = -2f;
    private bool isFiring;
    //private AudioSource gunSound;
    private Ammo ammo;
    private InputManager inputManager;

    private void Awake()
    {
        inputManager = new InputManager();
        inputManager.Player.Enable();
        //gunSound = GetComponent<AudioSource>();
        ammo = GetComponent<Ammo>();
    }

    private void OnEnable()
    {
        inputManager.Player.Shoot.performed += Shoot;
        inputManager.Player.Shoot.canceled += Shoot_canceled;
    }

    private void OnDisable()
    {
        inputManager.Player.Shoot.performed -= Shoot;
        inputManager.Player.Shoot.canceled -= Shoot_canceled;
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

    if (fpsCam == null)
        {
            Debug.LogError("¡Asigna la fpsCam en el Inspector!");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
    {
            if (hit.transform.CompareTag("Enemy"))
        {
                Component damageable = hit.transform.GetComponent(typeof(IDamageable));

            if (damageable)
            {
                    GameFunctions.Attack(damageable, damage);

                // Validar si el enemigo tiene Partículas
                ParticleSystem ps = hit.transform.GetComponentInChildren<ParticleSystem>();
                    if (ps != null) ps.Play();

                // Validar si el enemigo tiene la IA
                EnemyAI enemy = hit.transform.GetComponent<EnemyAI>();
                    if (enemy != null) enemy.OnDamageTaken();
            }
            }
        }
    }

    private void Shoot(InputAction.CallbackContext context)
    {
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
}
