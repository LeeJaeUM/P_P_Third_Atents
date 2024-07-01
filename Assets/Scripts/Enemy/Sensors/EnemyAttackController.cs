using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class EnemyAttackController : EnemySensorBase
{
    //공격범위 안에 들어오는지 확인 하는 스크립트

    private BoxCollider2D[] attackColliders;

    public int curPatern = 0;

    public Action<ICombat.IDamage> onAttack0;
    public Action<ICombat.IDamage> onAttack1;
    public Action<ICombat.IDamage> onAttack2;
    public Action<ICombat.IDamage> onAttack3;
    public Action<ICombat.IDamage> onAttack4;
    public Action<ICombat.IDamage> onAttack5;
    public Action<ICombat.IDamage> onAttack6;

    public Action onStunned;

    private E_AtkEnter[] e_AtkEnters;
    

    private void Start()
    {
        //enemyController에서 공격 델리게이트 보내면 공격범위 활성화 : 애니메이션 이벤트 사용 예정
        enemy.onAttack_Moment += () =>
        {
            StartCoroutine(ColliderOnOff_Moment());
        };

        enemy.onAttack_Continue += ColliderOnOff_ContinueFunc;

        enemy.onPaternChange += PaternChange;

        rangeCollider.enabled = true;

        //공격 범위들 배열에 넣기
        attackColliders = GetComponentsInChildren<BoxCollider2D>();


        //모든 공격 콜라이더 비활성화
        foreach(var attack in attackColliders)
        {
            attack.enabled = false;
        }

        //각각 콜라이더에 액션 할당
        e_AtkEnters = GetComponentsInChildren<E_AtkEnter>();

        foreach (var e in e_AtkEnters)
        {
            e.onDamage += OnActionPush;
        }
    }

    private void PaternChange(int atkIndex)
    {
        curPatern = atkIndex;
        rangeCollider = attackColliders[atkIndex];
        offsetX = Mathf.Abs( rangeCollider.offset.x);
        offsetY = Mathf.Abs( rangeCollider.offset.y);
    }


    /// <summary>
    /// 할당된 액션이 실행되면 현재 패턴에 따라서 데미지 함수 실행
    /// 어느 콜라이더가 닿았는지는 생각안하고 패턴 번호만 가지고 판단함
    /// </summary>
    /// <param name="damageable"></param>
    void OnActionPush(ICombat.IDamage damageable)
    {
        // 일반 공격상태일때
        if (enemy.ParryState == Enums.ParryState.None)
        {
            Debug.Log("일반공격함");
            AttackDeligate(damageable);
        }
        else
        {
            Debug.Log("특수패리공격함");
            //공격시 특수패리 가능 상태일때
            //상대도 동일한 특수패리 가능 상태라면
            if (enemy.ParryState == damageable.ParryState)
            {
                //모든 공격 콜라이더 비활성화
                foreach (var attack in attackColliders)
                {
                    attack.enabled = false;
                }
                //스턴 상태로 변경
                onStunned?.Invoke();
            }
            else
            {
                // 아니라면 데미지 입히기
                AttackDeligate(damageable);
            }
        }
       

    }

    private void AttackDeligate(ICombat.IDamage damageable)
    {
        //적이 공격을 성공하면 현재 패턴에 맞는 데미지를 주라는 액션 보냄
        switch (curPatern)
        {
            case 0:
                onAttack0?.Invoke(damageable);
                break;
            case 1:
                onAttack1?.Invoke(damageable);
                break;
            case 2:
                onAttack2?.Invoke(damageable);
                break;
            case 3:
                onAttack3?.Invoke(damageable);
                break;
            case 4:
                onAttack4?.Invoke(damageable);
                break;
            case 5:
                onAttack5?.Invoke(damageable);
                break;
            case 6:
                onAttack6?.Invoke(damageable);
                break;
        }
    }

    IEnumerator ColliderOnOff_Moment()
    {
        BoxCollider2D tempColldier2D = rangeCollider;
        tempColldier2D.enabled = true;
        yield return new WaitForSeconds(0.05f);
        tempColldier2D.enabled = false;
    }

    private void ColliderOnOff_ContinueFunc(float maintenanceTime)
    {
        StartCoroutine(ColliderOnOff_Continue(maintenanceTime));
    }

   /// <summary>
   /// 지속 시간 동안 계속 공격콜라이더를 활성화 시켜둠
   /// </summary>
   /// <param name="maintenanceTime"></param>
   /// <returns></returns>
   IEnumerator ColliderOnOff_Continue(float maintenanceTime)
    {
        // 공격이 종료되기 전에 패턴이 바뀔 가능성 배제
        BoxCollider2D tempColldier2D = rangeCollider;
        tempColldier2D.enabled = true;
        yield return new WaitForSeconds(maintenanceTime);
        tempColldier2D.enabled = false;
    }
}
