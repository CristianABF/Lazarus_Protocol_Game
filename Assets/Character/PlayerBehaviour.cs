using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public float playerHealth = 100f;
    private bool isDead = false;

    private void Awake()
    {
        playerHealth = 100f;
    }

    private void Update()
    {
        PlayerDead();
    }

    private void PlayerDead()
    {
        if (playerHealth <= 0f)
        {
            isDead = true;
            Debug.Log("You have been killed!");

            playerHealth = 100f;
            Debug.Log("Salud restaurada");
        }
    }

    public void ResetState() {isDead = false;}
    public bool GetState() {return isDead;}
}
