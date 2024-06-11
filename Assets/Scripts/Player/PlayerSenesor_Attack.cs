using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSenesor_Attack : MonoBehaviour
{
    private PlayerController controller;

    private void Awake()
    {
        controller = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌한 오브젝트에서 IDamage 인터페이스를 얻습니다.
        ICombat.IDamage damageable = collision.GetComponent<ICombat.IDamage>();

        // IDamage 인터페이스가 구현되어 있는지 확인합니다.
        if (damageable != null)
        {
            controller.Attack(damageable);
        }
    }

}
