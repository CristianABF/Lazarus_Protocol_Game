using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Weapons")]
    [SerializeField] private GameObject[] weaponObjects;

    private void Awake()
    {
        ManageSingleton();
    }

    private void ManageSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ActivateWeaponObject(int slot)
    {
        for (int i = 0; i < weaponObjects.Length; i++)
        {
            if (i == slot)
            {
                weaponObjects[i].SetActive(true);
            }
            else
            {
                weaponObjects[i].SetActive(false);
            }
        }
    }
}
