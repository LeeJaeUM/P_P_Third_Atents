using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Plant : EnemyController
{
    protected override void Awake()
    {
        base.Awake();

        timeAttackElaped = 1.6f;
        attackDelay = 2.0f;
    }

    // 공격 방식이 하나만 있는 꽃 몬스터
    protected override void AttackTry()
    {
        timeSinceAttack = 0.0f;

        animator.SetTrigger("Attack");

        StartCoroutine(Attacking_Physics());
    }

    private void DoAttack()
    {
        //공격 눌렀다고 알림 = 공격 범위 활성화
        onAttack?.Invoke();
    }
}
