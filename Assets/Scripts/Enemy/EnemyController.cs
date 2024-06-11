using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : CharacterBase
{
    /// <summary>
    /// HP 설정 및 확인용 프로퍼티 오버라이드함
    /// </summary>
    override public float CurrentHealth 
    {
        get => currentHealth;
        set
        {
            currentHealth = value;
            if (currentHealth <= 0)
            {
                State = BehaviorState.Dead; // HP가 0이하면 사망
            }
        }
    }

    /// <summary>
    /// 사망시 실행될 델리게이트
    /// </summary>
    public Action<EnemyController> onDie;

    // 상태 관련 ------------------------------------------------------------------------------------------
    public enum BehaviorState : byte
    {
        Patrol = 0, // 배회상태. 주변을 왔다갔다한다.
        Chase,      // 추적상태. 플레이어가 마지막으로 목격된 장소를 향해 계속 이동한다.
        Find,       // 탐색상태. 추적 도중에 플레이어가 시야에서 사라지면 두리번 거리며 주변을 찾는다.
        Attack,     // 공격상태. 추적 상태일 때 플레이어가 일정범위안에 들어오면 일정 주기로 공격을 한다.
        Dead        // 사망상태. 죽는다.(일정 시간 후에 재생성)
    }

    /// <summary>
    /// 적의 현재 상태
    /// </summary>
    [SerializeField] private BehaviorState state = BehaviorState.Dead;

    /// <summary>
    /// 적의 상태 확인 및 설정용 프로퍼티
    /// </summary>
    BehaviorState State
    {
        get => state;
        set
        {
            if (state != value)          // 상태가 달라지면
            {
                OnStateExit(state);     // 이전 상태에서 나가기 처리 실행
                state = value;
                OnStateEnter(state);    // 새 상태에 들어가기 처리 실행
            }
        }
    }

    /// <summary>
    /// 각 상태가 되었을때 상태별 업데이트 함수를 저장하는 델리게이트(함수포인터의 역할)
    /// </summary>
    Action onUpdate = null;

    // 이동 관련 ----------------------------------------------------------------------------------------

    /// <summary>
    /// 이동 속도(배회 및 찾기 상태에서 사용)
    /// </summary>
    public float walkSpeed = 2.0f;

    /// <summary>
    /// 이동 속도(추적 및 공격 상태에서 사용)
    /// </summary>
    public float runSpeed = 7.0f;

    // 공격 관련 --------------------------------------------------------------------------------------------

    /// <summary>
    /// 공격 대상
    /// </summary>
    PlayerController attackTarget = null;

    /// <summary>
    /// 공격 시간 간격
    /// </summary>
    public float attackInterval = 1.0f;

    /// <summary>
    /// 공격 시간 측정용
    /// </summary>
    float attackElapsed = 0;

    /// <summary>
    /// 공격력 패널티 정도
    /// </summary>
    float attackPowerPenalty = 0;

    // 탐색 관련 -------------------------------------------------------------------------------------------

    /// <summary>
    /// 탐색 상태에서 배회 상태로 돌아가기까지 걸리는 시간
    /// </summary>
    public float findTime = 5.0f;

    /// <summary>
    /// 탐색 진행 시간
    /// </summary>
    float findTimeElapsed = 0.0f;

    /// <summary>
    /// 추적 대상
    /// </summary>
    Transform chaseTarget = null;

    //컴포넌트
    private EnemySensor enemySensor = null;

    // UnityEvent Functions--------------------------------------------------------------------------------------------------------
    #region UnityEvent Functions

    private void Awake()
    {

        //AttackSensor attackSensor = child.GetComponent<AttackSensor>();
        //attackSensor.onSensorTriggered += (target) =>
        //{
        //    if (attackTarget == null)    // Attack 상태에서 한번만 실행됨
        //    {
        //        attackTarget = target.GetComponent<Player>();
        //        attackTarget.onDie += ReturnWander;
        //        State = BehaviorState.Attack;
        //    }
        //};
    }

    private void Start()
    {
        State = BehaviorState.Patrol;
        enemySensor = transform.GetChild(0).GetComponent<EnemySensor>();
        enemySensor.onSensorTriggered += () => State = BehaviorState.Chase;
    }

    private void OnEnable()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            chaseTarget = other.transform;
            //Debug.Log("In : " + chaseTarget);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Out : " + chaseTarget);
            //chaseTarget = null;
        }
    }


    private void Update()
    {
        onUpdate();
    }

    #endregion

    #region State Updates

    void Update_Patrol()
    {
        //if (FindPlayer())
        //{
        //    State = BehaviorState.Chase;                    // 플레이어를 찾았으면 Chase 상태로 변경
        //}
        //else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        //{
        //    agent.SetDestination(GetRandomDestination());   // 목적지에 도착했으면 다시 랜덤 위치로 이동
        //}
    }

    void Update_Chase()
    {
        //if (IsPlayerInSight(out Vector3 position))
        //{
        //    agent.SetDestination(position); // 마지막 목격 장소를 목적지로 새로 설정
        //}
        //else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        //{
        //    // 플레이어가 안보이고 마지막 목격지에 도착했다 => 찾기 상태로 전화
        //    State = BehaviorState.Find;
        //}
    }

    void Update_Find()
    {
        findTimeElapsed += Time.deltaTime;
        if (findTimeElapsed > findTime)
        {
            State = BehaviorState.Patrol;   // 일정 시간이 지날때까지 플레이어를 못찾음 -> 배회 상태로 변경
        }
        else if (FindPlayer())
        {
            State = BehaviorState.Chase;    // 플레이어 찾았다 -> 추적
        }
    }

    void Update_Attack()
    {

    }

    void Update_Dead()
    {

    }

    #endregion

    /// <summary>
    /// 특정 상태가 되었을 때의 처리를 실행하는 함수
    /// </summary>
    /// <param name="newState">새 상태</param>
    void OnStateEnter(BehaviorState newState)
    {
        switch (newState)
        {
            case BehaviorState.Patrol:
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

    /// <summary>
    /// 특정 샅애에서 나갈때의 처리를 실행하는 함수
    /// </summary>
    /// <param name="oldState">옛 상태</param>
    void OnStateExit(BehaviorState oldState)
    {
        switch (oldState)
        {
            case BehaviorState.Find:
                break;
            case BehaviorState.Attack:
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

        /*
         switch (oldState)
        {            
            case BehaviorState.Find:
                agent.angularSpeed = 120.0f;
                StopAllCoroutines();
                break;
            case BehaviorState.Attack:
                attackTarget.onDie -= ReturnWander;
                attackTarget = null;
                break;
            case BehaviorState.Dead:
                gameObject.SetActive(true);
                HP = maxHP;
                break;
            default:
            //case BehaviorState.Wander:    // 사용하지 않음
            //case BehaviorState.Chase:
                break;
        }
         */
    }


    /// <summary>
    /// 공격 당함을 처리하는 함수
    /// </summary>
    /// <param name="hit">맞은 부위</param>
    /// <param name="damage">데미지</param>
    public void OnAttacked(float damage)
    {
        // 맞으면 즉시 추적에 돌입한다.
    }

    /// <summary>
    /// 플레이어를 찾는 시도를 함수
    /// </summary>
    /// <returns>true면 플레이어를 찾았다. false면 못찾았다.</returns>
    bool FindPlayer()
    {
        return false;
    }

    /// <summary>
    /// 플레이어가 시야범위 안에 있는지 확인하는 함수
    /// </summary>
    /// <param name="position">플레이어가 시야범위 안에 있을 때 플레이어의 위치</param>
    /// <returns>true면 시야범위 안에 있다. false면 시야범위 안에 없다.</returns>
    bool IsPlayerInSight(out Vector3 position)
    {
        position = Vector3.zero;
        return false;
    }

    /// <summary>
    /// 주변을 두리번 거리는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveAround()
    {
        yield return null;
    }


    /// <summary>
    /// 적을 리스폰 시키는 함수
    /// </summary>
    /// <param name="spawnPosition">리스폰할 위치</param>
    public void Respawn(Vector3 spawnPosition)
    {
        //agent.Warp(spawnPosition);
        State = BehaviorState.Patrol;
    }

    /// <summary>
    /// 적이 드랍할 아이템의 종류를 나타내는 enum
    /// </summary>
    enum ItemTable : byte
    {
        Heal,           // 힐 아이템
        AssaultRifle,   // 돌격소총
        Shotgun,        // 샷건
        Random          // 랜덤
    }

    /// <summary>
    /// 아이템을 드랍하는 함수
    /// </summary>
    /// <param name="table">드랍할 아이템</param>
    void DropItem(ItemTable table = ItemTable.Random)
    {

    }

}
