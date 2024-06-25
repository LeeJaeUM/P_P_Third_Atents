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

    private float curOnTime = 0;
    private float offTime = 2;

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

        //공격범위가 꺼지지 않았을떄를 대비한 비활성화 조건 추가
        if (attackRangeCol.enabled)
        {
            curOnTime += Time.deltaTime;
        }

        if (curOnTime > offTime)
        {
            attackRangeCol.enabled = false;
            curOnTime = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌한 오브젝트에서 IDamage 인터페이스를 얻습니다.
        ICombat.IDamage damageable = collision.GetComponent<ICombat.IDamage>();
        ICombat.IParryState enemyParryState = collision.GetComponent<ICombat.IParryState>();

        // IDamage 인터페이스가 구현되어 있는지 확인합니다.
        if (damageable != null)
        {
            controller.Attack(damageable);
        }

        if(enemyParryState != null)
        {
            //enemy의 parrystate인터페이스의 함수 실행
            enemyParryState.ParrySec();
        }
    }

    IEnumerator ColliderOnOff()
    {
        attackRangeCol.enabled = true;
        yield return new WaitForSeconds(0.05f);
        attackRangeCol.enabled = false;
    }

}
