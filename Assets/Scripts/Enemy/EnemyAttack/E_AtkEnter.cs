using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_AtkEnter : MonoBehaviour
{

    public Action<ICombat.IDamage> onDamage;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ICombat.IDamage damageable = collision.GetComponent<ICombat.IDamage>();
        if (damageable != null)
        {
            onDamage?.Invoke(damageable);
        }
    }
}
