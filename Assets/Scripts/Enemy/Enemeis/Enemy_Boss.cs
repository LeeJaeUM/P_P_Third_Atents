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


    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        chaseDis = 20.0f;
        curRunSpeed = 2;
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


        MoveTowards(lastSeenPosition, curRunSpeed); // 마지막 위치로 이동
    }

    #region AttackPaternFunc

    protected override void AttackAnimationStart()
    {
        switch (attackPattern)
        {
            case Enums.AttackPatern.Attack_0: animator.SetTrigger(Attack0_Hash); break;
            case Enums.AttackPatern.Attack_1: animator.SetTrigger(Attack1_Hash);
                Set_PowerAttack();
                break;
            case Enums.AttackPatern.Attack_2: animator.SetTrigger(Attack2_Hash);
                Set_DashAttack();
                break;
            case Enums.AttackPatern.Attack_3: animator.SetTrigger(Attack3_Hash); break;
            case Enums.AttackPatern.Attack_4: animator.SetTrigger(Attack4_Hash); break;
            case Enums.AttackPatern.Attack_5: animator.SetTrigger(Attack5_Hash); break;
            case Enums.AttackPatern.Attack_6: animator.SetTrigger(Attack6_Hash); break;
        }
    }

    private void Set_PowerAttack()
    {
        AttackStartSetting(2, 1f, 1);

        rigid.velocity = Vector3.zero;
    }

    private void Set_DashAttack()
    {
        parryState = Enums.ParryState.DashAttack;
        AttackStartSetting(3, 0.5f, 12);

        rigid.velocity = Vector3.zero;
    }

    /// <summary>
    /// 공격 시작 시 변수값 설정할 함수 / 공격 대기시간, 이동공격의 이동 시간, 공격 시 앞으로 나아갈 속도
    /// </summary>
    /// <param name="delay">공격 대기시간</param>
    /// <param name="elapedTime">이동공격의 이동 시간</param>
    /// <param name="force">공격 시 앞으로 나아갈 속도</param>
    private void AttackStartSetting(float delay, float elapedTime, float force = 1.0f)
    {
        curAttackDelay = delay;
        curAttackMoveTime = elapedTime;
        attackMoveSpeed = force;
    }

    /// <summary>
    /// 공격 종료 시 기본 설정으로 되돌림
    /// </summary>
    private void AttackEndRefresh()
    {
        curAttackDelay = defaultAttackDelay;
        curAttackMoveTime = defaultAttackMoveTime;
        attackMoveSpeed = defaultAttackForce;

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
        target.TakeDamage(attackPower * 0.8f, transform.position.x);
    }

    /// <summary>
    /// 대시공격
    /// </summary>
    /// <param name="target"></param>
    protected override void Attakc2_Damage(ICombat.IDamage target)
    {
        target.TakeDamage(attackPower * 4.0f, transform.position.x, false);
    }

    #endregion

#if UNITY_EDITOR

    public void TestAttack3()
    {
        State = BehaviorState.Attack;
        Set_DashAttack();
    }

#endif
}
