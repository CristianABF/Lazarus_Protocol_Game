using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ammo : MonoBehaviour
{
    [SerializeField] private int ammoAmount = 10;
    [SerializeField] private int reloadAmount = 10;
    [SerializeField] private int clips = 5;

    private int maxAmmo;
    private InputManager inputManager;

    private void Awake()
    {
        maxAmmo = ammoAmount;
        inputManager = new InputManager();
        inputManager.Player.Enable();
    }

    private void OnEnable()
    {
        inputManager.Player.Reload.performed += Reload;
    }

    private void OnDisable()
    {
        inputManager.Player.Reload.performed -= Reload;
    }

    public int GetCurrentAmmo()
    {
        return ammoAmount;
    }

    public int GetCurrentClips()
    {
        return clips;
    }

    public void ReduceCurrentAmmo()
    {
        ammoAmount--;
    }

    public int GetMaxAmmo()
    {
        return maxAmmo;
    }

    private void Reload(InputAction.CallbackContext obj)
    {
        if (clips > 0)
        {
            clips--;
            SetAmmo(reloadAmount);
        }
    }

    private void SetAmmo(int reloadAmount)
    {
        ammoAmount = reloadAmount;
    }
}
