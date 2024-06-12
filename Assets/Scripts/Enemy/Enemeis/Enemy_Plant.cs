using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Plant : EnemyController
{
    /// <summary>
    /// 디버그용 공격범위 시각적 표시
    /// </summary>
    private SpriteRenderer attackSpriteRenderer;

    protected override void Awake()
    {
        base.Awake();

        timeAttackElaped = 1.6f;
        attackDelay = 2.0f;

        attackSpriteRenderer = transform.GetChild(3).GetComponent<SpriteRenderer>();
    }

    // 공격 방식이 하나만 있는 꽃 몬스터
    protected override void AttackTry()
    {
        timeSinceAttack = 0.0f;

        animator.SetTrigger("Attack");

        StartCoroutine(Attacking_Physics());
    }

    protected override IEnumerator Attacking_Physics()
    {
        attackSpriteRenderer.enabled = true;
        isAttacking = true;
        rigid.velocity = new Vector2(attackForce * FacingDirection, rigid.velocity.y);
        yield return new WaitForSeconds(timeAttackElaped);
        isAttacking = false;
    }

    private void DoAttack()
    {
        //공격 눌렀다고 알림 = 공격 범위 활성화
        onAttack?.Invoke();
        attackSpriteRenderer.enabled = false;
    }
}
