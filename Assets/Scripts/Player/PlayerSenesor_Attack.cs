using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSenesor_Attack : MonoBehaviour
{
    private PlayerController controller;
    private BoxCollider2D attackRangeCol;

    private float offsetX = 0;
    private float offsetY = 0;

    private void Awake()
    {
        controller = GetComponentInParent<PlayerController>();
        controller.onAttack += () =>
        {
            StartCoroutine(ColliderOnOff());
        };

        attackRangeCol = GetComponent<BoxCollider2D>();
        offsetX = attackRangeCol.offset.x;
        offsetY = attackRangeCol.offset.y;
    }

    private void Update()
    {
        attackRangeCol.offset = new Vector2(offsetX * controller.FacingDirection, offsetY);
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

    IEnumerator ColliderOnOff()
    {
        attackRangeCol.enabled = true;
        yield return new WaitForSeconds(0.05f);
        attackRangeCol.enabled = false;
    }

}
