using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class EnemySensor_Attack : EnemySensorBase
{
    private bool isFindPlayer = false;

    private void Start()
    {
        //enemyController에서 공격 델리게이트 보내면 공격범위 활성화 : 애니메이션 이벤트 사용 예정
        enemy.onAttack += () =>
        {
            StartCoroutine(ColliderOnOff());
        };

        enemy.onExitAttackState += () =>
        {
            // Attack state에서 벗어나면 다시 탐색용으로 초기화
            isFindPlayer = false;
            rangeCollider.enabled = true;
        };

        rangeCollider.enabled = true;
    }

    protected override void Update()
    {
        base.Update();
        if (!isFindPlayer)
        {
            //탐색 모드일때 항시 켜져있도록 update로 고정
            rangeCollider.enabled = true;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if(!isFindPlayer)
        {
            //플레이어를 찾지 못했다면 범위 체크로 사용
            if (collision.CompareTag("Player"))
            {
                //플레이어를 찾으면 공격으로 변환
                isFindPlayer = true;
                rangeCollider.enabled = false;
                enemy.SetAttackState();
            }
        }
        else
        {
            //플레이어를 찾았을 때 공격한다. 콜라이더는 enemy로부터 델리게이트를 받아 비/활성화하여 데미지 줌
            // 충돌한 오브젝트에서 IDamage 인터페이스를 얻습니다.
            ICombat.IDamage damageable = collision.GetComponent<ICombat.IDamage>();

            // IDamage 인터페이스가 구현되어 있는지 확인합니다.
            if (damageable != null)
            {
                enemy.Attack(damageable);
            }
        }

    }
    IEnumerator ColliderOnOff()
    {
        rangeCollider.enabled = true;
        yield return new WaitForSeconds(0.05f);
        rangeCollider.enabled = false;
    }
}
