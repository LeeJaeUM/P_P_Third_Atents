using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Boss : EnemyController
{
    //Boss의 Find는 그로기로 사용

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
        base.Update_Find();
    }

    protected override void Update_Attack()
    {
        base.Update_Attack();
    }


}
