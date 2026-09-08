using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameFunctions
{
    public static void Attack(Component damageable, float baseDamage)
    {
        if (damageable)
        {
            (damageable as IDamageable).TakeDamage(baseDamage);
        }
    }
}
