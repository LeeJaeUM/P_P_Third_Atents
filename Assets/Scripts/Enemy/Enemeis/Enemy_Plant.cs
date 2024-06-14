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

        curTimeAttackElaped = 1.6f;
        curAttackDelay = 2.0f;

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
        yield return new WaitForSeconds(curTimeAttackElaped);
        isAttacking = false;
    }

    protected override void DoAttack()
    {
        base.DoAttack();    //액션 하나 onAttack?.Invoke();
        attackSpriteRenderer.enabled = false;
    }

}
