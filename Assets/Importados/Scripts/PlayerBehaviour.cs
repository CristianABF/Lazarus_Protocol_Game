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
        if (playerHealth <= 0f)
        {
            isDead = true;
            Debug.Log("You have been killed!");
        }
    }
}
