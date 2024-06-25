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
    private bool isPaternOn = false;
    [SerializeField]
    private float turnLockDuration = 0.1f;

    protected enum AttackPattern
    {
        NormalAttack = 0,
        PowerAttack,
        Dash,
        Missile
    }
    [SerializeField]
    protected AttackPattern attackPattern = AttackPattern.NormalAttack;

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

    protected override void OnStateExit(BehaviorState oldState)
    {
        switch (oldState)
        {
            case BehaviorState.Find:
                findTimeElapsed = 0;    //다시 Find로 돌아가을때를 위해 탐색시간 초기화
                break;
            case BehaviorState.Attack:
                onExitAttackState?.Invoke();
                break;
            case BehaviorState.Dead:
                gameObject.SetActive(true);
                CurrentHealth = MaxHealth;
                break;
            default:
                //case BehaviorState.Patrol:    // 사용하지 않음
                //case BehaviorState.Chase:
                break;
        }
    }

    protected override void OnStateEnter(BehaviorState newState)
    {
        switch (newState)
        {
            case BehaviorState.Patrol:
                isRightPatrol = true;
                FacingDirection = 1;
                onUpdate = Update_Patrol;
                break;
            case BehaviorState.Chase:
                onUpdate = Update_Chase;
                break;
            case BehaviorState.Find:
                onUpdate = Update_Find;
                break;
            case BehaviorState.Attack:
                onUpdate = Update_Attack;
                break;
            case BehaviorState.Dead:
                onUpdate = Update_Dead;
                break;
            default:
                break;
        }
    }
    protected override void Update_Patrol()
    {
        State = BehaviorState.Chase;
    }

    protected override void Update_Chase()
    {
        SetFacingDirection();

        MoveTowards(lastSeenPosition); // 마지막 위치로 이동
    }

    protected override void Update_Find()
    {
        findTimeElapsed += Time.deltaTime;
        if (findTimeElapsed > findTime)
        {
            State = BehaviorState.Chase;   // 그로기 시간이 끝나면 다시 Chase 시작
        }
    }

    protected override void Update_Attack()
    {
        //공격 딜레이용 시간변수
        timeSinceAttack += Time.deltaTime;

        //공격중엔 회전 안함
        if (!isAttacking)
        {
            SetFacingDirection();
        }

        // 플레이어와의 x축 거리 계산 후 공격거리보다 크고 공격중이 아니면 chase로 변경
        if (!isPaternOn && !isAttacking && Mathf.Abs(transform.position.x - player.transform.position.x) > attackDistance)
        {
            Debug.Log("Attack에서 Chase로 바뀐다");
            State = BehaviorState.Chase;
        }
        else
        {
            // 공격 로직
            if (timeSinceAttack > curAttackDelay)
            {
                //공격 대기 시간 초기화
                timeSinceAttack = 0.0f;
                //패턴 시작 알림
                isPaternOn = true;
                switch (attackPattern)
                {
                    case AttackPattern.NormalAttack: AttackTry(); break;
                    case AttackPattern.PowerAttack: AttackTry(); break;
                    case AttackPattern.Missile: AttackTry(); break;
                    case AttackPattern.Dash: DashAttack(); break;
                }
            }
        }
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

    #region AttackPaternFunc

    private void AttackPhysicsFunc()
    {
        StartCoroutine(Attacking_Physics());
    }


    protected override void AttackTry()
    {

        animator.SetTrigger("Attack");

        StartCoroutine(Attacking_Physics());
    }

    private void DashAttack()
    {
        parryState = Enums.ParryState.DashAttack;
        AttackStartSetting(3, 0.3f, 10);

        rigid.velocity = Vector3.zero;
        animator.SetTrigger("Attack 3");

        //공격 종료 후 다시 원래대로 복귀 - 코루틴으로 활용
        StartCoroutine(AttackEndRefresh());

    }

    private void IsPaternOff()
    {
        isPaternOn = false;
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

        parryState = Enums.ParryState.None;
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
