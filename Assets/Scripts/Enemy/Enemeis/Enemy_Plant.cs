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

        curAttackMoveTime = 1.6f;
        curAttackDelay = 2.0f;

        attackSpriteRenderer = transform.GetChild(3).GetComponent<SpriteRenderer>();
    }

    protected override IEnumerator Attacking_Physics()
    {
        attackSpriteRenderer.enabled = true;
        isTurnable = true;
        rigid.velocity = new Vector2(attackMoveSpeed * FacingDirection, rigid.velocity.y);
        yield return new WaitForSeconds(curAttackMoveTime);
        isTurnable = false;
    }

    protected override void DoAttack_Collider_Moment()
    {
        base.DoAttack_Collider_Moment();    //액션 하나 onAttack?.Invoke();
        attackSpriteRenderer.enabled = false;
    }

}
