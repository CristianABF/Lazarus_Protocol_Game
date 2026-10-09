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

    // NUEVO
    [SerializeField] private int maxClips = 5;

    private int maxAmmo;

    private void Awake()
    {
        maxAmmo = ammoAmount;
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

    // NUEVO: Permite que agentes externos consulten el límite sin poder modificarlo
    public int GetMaxClips()
    {
        return maxClips;
    }

    public void ExecuteReload()
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

    // NUEVO: El único método autorizado para alterar la reserva desde afuera
    public void RecibirCargadores(int cantidadSuministrada)
    {
        clips += cantidadSuministrada;

        // Tope matemático estricto
        if (clips > maxClips)
        {
            clips = maxClips;
        }
    }
}