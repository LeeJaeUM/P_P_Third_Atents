using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class EnemyAttackController : EnemySensorBase
{
    //공격범위 안에 들어오는지 확인 하는 스크립트

    private bool isFindPlayer = false;

    private BoxCollider2D[] attackColliders;

    public Action<ICombat.IDamage> onAttack0;
    public Action<ICombat.IDamage> onAttack1;
    public Action<ICombat.IDamage> onAttack2;
    public Action<ICombat.IDamage> onAttack3;
    public Action<ICombat.IDamage> onAttack4;
    public Action<ICombat.IDamage> onAttack5;
    public Action<ICombat.IDamage> onAttack6;
    

    private void Start()
    {
        //공격 범위들 배열에 넣기
        attackColliders = GetComponentsInChildren<BoxCollider2D>();

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

        enemy.onPaternChange += PaternChange;

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
    private void PaternChange(int obj)
    {
        throw new NotImplementedException();
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
            onAttack0?.Invoke(damageable);
        }

    }
    IEnumerator ColliderOnOff()
    {
        rangeCollider.enabled = true;
        yield return new WaitForSeconds(0.05f);
        rangeCollider.enabled = false;
    }
}
