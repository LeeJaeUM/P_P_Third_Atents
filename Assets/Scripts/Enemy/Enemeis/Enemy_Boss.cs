using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Boss : EnemyController
{
    //Boss의 Find는 그로기로 사용

    [SerializeField]
    private float dashAttackForce = 8.0f;
    private float defaultAttackForce = 2.0f;

    [SerializeField]
    private float turnLockDuration = 0.1f;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        chaseDis = 20.0f;
    }

    protected override void Update()
    {
        base.Update();
        lastSeenPosition = player.transform.position;
    }

    protected override void Update_Patrol()
    {
        State = BehaviorState.Chase;
    }

    protected override void Update_Chase()
    {
        SetFacingDirection();        
        
        // 현재 위치의 x가 플레이어의 x에 거의 도달했는지 확인
        if (Mathf.Abs(transform.position.x - player.transform.position.x) < attackDistance)
        {
            State = BehaviorState.Attack;
        }


        MoveTowards(lastSeenPosition); // 마지막 위치로 이동
    }

    protected override IEnumerator Attacking_Physics()
    {
        //이떄부터 회전 불가
        isAttacking = true;
        float temp = 0;
        // 즉시 공격하지말고 0.1초 대기 후 공격
        yield return new WaitForSeconds(turnLockDuration);

        //회전 금지 시간 동안 이동 
        while (temp < curTimeAttackElaped)
        {
            temp += Time.deltaTime;
            rigid.velocity = new Vector2(attackForce * FacingDirection, rigid.velocity.y);
            yield return null;
        }
        // 즉시 종료하지말고 0.1초 대기
        yield return new WaitForSeconds(turnLockDuration);
        isAttacking = false;
    }
    private void AttackPhysicsFunc()
    {
        StartCoroutine(Attacking_Physics());
    }


    #region AttackPaternFunc

    protected override void AttackAnimationStart()
    {
        switch (attackPattern)
        {
            case Enums.AttackPatern.Attack_0: animator.SetTrigger(Attack0_Hash); break;
            case Enums.AttackPatern.Attack_1: animator.SetTrigger(Attack1_Hash); break;
            case Enums.AttackPatern.Attack_2: animator.SetTrigger(Attack2_Hash);
                DashAttack();
                break;
            case Enums.AttackPatern.Attack_3: animator.SetTrigger(Attack3_Hash); break;
            case Enums.AttackPatern.Attack_4: animator.SetTrigger(Attack4_Hash); break;
            case Enums.AttackPatern.Attack_5: animator.SetTrigger(Attack5_Hash); break;
            case Enums.AttackPatern.Attack_6: animator.SetTrigger(Attack6_Hash); break;
        }
    }

    private void DashAttack()
    {
        parryState = Enums.ParryState.DashAttack;
        AttackStartSetting(3, 0.3f, 10);

        rigid.velocity = Vector3.zero;

        //공격 종료 후 다시 원래대로 복귀 - 코루틴으로 활용
        StartCoroutine(AttackEndRefresh());
    }

    /// <summary>
    /// 공격 시작 시 변수값 설정할 함수
    /// </summary>
    /// <param name="delay">공격 대기시간</param>
    /// <param name="elapedTime">공격 중 회전 방지 시간</param>
    /// <param name="force">공격 시 앞으로 나아갈 힘</param>
    private void AttackStartSetting(float delay, float elapedTime, float force)
    {
        curAttackDelay = delay;
        curTimeAttackElaped = elapedTime;
        attackForce = force;
    }

    /// <summary>
    /// 공격 종료 시 기본 설정으로 되돌림
    /// </summary>
    private IEnumerator AttackEndRefresh()
    {
        yield return new WaitForSeconds(curAttackDelay);
        curAttackDelay = defaultAttackDelay;
        curTimeAttackElaped = defaultTimeAttackElaped;
        attackForce = defaultAttackForce;

        //parryState = Enums.ParryState.None;
    }

    #endregion

    #region 공격패턴마다 정해진 데미지 주는 함수 EnemyAttackController에서 실행됨

    protected override void AttackActionRegistering()
    {
        enemyAttackController.onAttack0 += Attack0_Damage;  //부모 클래스 함수 사용
        enemyAttackController.onAttack1 += Attakc1_Damage;
        enemyAttackController.onAttack2 += Attakc2_Damage;
    }

    /// <summary>
    /// PowerAttack
    /// </summary>
    /// <param name="target"></param>
    protected override void Attakc1_Damage(ICombat.IDamage target)
    {
        Debug.Log("1 attack 했음  - boss");
        target.TakeDamage(attackPower * 0.8f, transform.position.x);
    }

    /// <summary>
    /// 대시공격
    /// </summary>
    /// <param name="target"></param>
    protected override void Attakc2_Damage(ICombat.IDamage target)
    {
        Debug.Log("2 했음 - boss");
        target.TakeDamage(attackPower * 4.0f, transform.position.x);
    }

    #endregion

#if UNITY_EDITOR

    public void TestAttack3()
    {
        State = BehaviorState.Attack;
        DashAttack();
    }

#endif
}
