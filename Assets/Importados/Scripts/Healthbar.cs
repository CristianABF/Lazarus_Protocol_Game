using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    [SerializeField] EnemyHealth enemyHealth;
    [SerializeField] Image foreground;

    void Update()
    {
        foreground.fillAmount = enemyHealth.health / enemyHealth.maxHealth;
    }
}
