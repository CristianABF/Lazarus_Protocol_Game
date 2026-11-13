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

    [SerializeField] private int maxClips = 5;

    private int maxAmmo;

    private void Awake()
    {
        maxAmmo = ammoAmount;

        // --- INYECCIÓN DE CARGADOR DE LA TIENDA ---
        if (ControladorPartida.Instancia != null)
        {
            // Ej. Pasar de 30 a 45 balas. Ajusta el máximo y rellena.
            maxAmmo += ControladorPartida.Instancia.balasExtraCargador;
            ammoAmount = maxAmmo;
        }
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
        // Si el truco está encendido, aborta la resta de balas
        if (CheatManager.municionInfinitaActiva) return;

        ammoAmount--;
    }

    public int GetMaxAmmo()
    {
        return maxAmmo;
    }

    //Permite que agentes externos consulten el límite sin poder modificarlo
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

    //El único método autorizado para alterar la reserva desde afuera
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