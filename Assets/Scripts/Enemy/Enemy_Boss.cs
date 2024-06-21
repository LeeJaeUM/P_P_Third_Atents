using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Boss : EnemyController
{
    //Boss의 Find는 그로기로 사용

    

    enum AttackPattern
    {
        NormalAttack = 0,
        PowerAttack,
        Dash,
        Missile
    }

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
        if (Mathf.Abs(transform.position.x - player.transform.position.x) > attackDistance && !isAttacking)
        {
            State = BehaviorState.Chase;
        }
        else
        {
            // 공격 로직
            if (timeSinceAttack > curAttackDelay)
            {
                AttackTry();
            }
        }
    }

    protected override void AttackTry()
    {

        timeSinceAttack = 0.0f;

        animator.SetTrigger("Attack");

        StartCoroutine(Attacking_Physics());
    }

    protected override IEnumerator Attacking_Physics()
    {
        return base.Attacking_Physics();
    }



}
