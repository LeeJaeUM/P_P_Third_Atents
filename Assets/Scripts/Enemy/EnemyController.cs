using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyController : CharacterBase
{
    /// <summary>
    /// HP 설정 및 확인용 프로퍼티 오버라이드함
    /// </summary>
    override public float CurrentHealth 
    {
        get => currentHealth;
        protected set
        {
            // 최소값은 0, 최대값은 maxHealth로 제한
            currentHealth = Mathf.Clamp(value, 0, maxHealth);

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

    // 상태 관련 -----------------------_______________-------------------------------------_______/////_______---------------------____________________------------------------------
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
    public float curWalkSpeed = 2.0f;
    public float defaultWalkSpeed = 2.0f;
    

    /// <summary>
    /// 이동 속도(추적 및 공격 상태에서 사용)
    /// </summary>
    public float curRunSpeed = 3.0f;
    public float defaultRunSpeed = 3.0f;

    // Patrol(순찰) 관련
    [Header("Patrol")]
    [SerializeField] private float leftPatrol = 0;
    [SerializeField] private float rightPatrol = 0;

    public float patrolRange = 3;
    [SerializeField] private bool isRightPatrol = true;


    [Header("Sprite")]
    /// <summary>
    /// 스프라이트가 기본적으로 오른쪽일때 true. facingDirection에 따라 sprite가 flip할때 기준을 정함
    /// </summary>
    public bool isSpriteRight = true;
    [SerializeField] private int facingDirection = 1;                // 캐릭터가 바라보는 방향
    public int FacingDirection
    {
        get => facingDirection;
        private set
        {
            if(facingDirection != value)
            {
                facingDirection = value;
                if(facingDirection > 0)
                    spriteRenderer.flipX = !isSpriteRight;
                else
                    spriteRenderer.flipX = isSpriteRight;
            }
        }
    }


    /// <summary>
    /// 마지막으로 플레이어를 본 위치. 서치센서가 인식했을 때 chase로 state를 바꾸면서 최신화함
    /// </summary>
    private Vector3 lastSeenPosition;               

    public float stoppingDistance = 1.8f;          //마지막으로 본 위치에 도달했다고 판정하는 거리

    // 공격 관련 ----------------------++++++++$$$$$$$$$$$+++++=-----------------------------------------------------

    [Header("Attack")]
    [SerializeField] protected int currentAttack = 0;         // 현재 공격 단계
    protected float timeSinceAttack = 0.0f;                   // 마지막 공격 이후 경과 시간
    [SerializeField] protected float curAttackDelay = 1.5f;      // 공격 대기 시간
    [SerializeField] protected float defaultAttackDelay = 1.5f;      // 공격 대기 시간
    [SerializeField] protected float attackForce = 2.0f;      // 공격 시 앞으로 나갈 거리
    public float attackDistance = 3.0f;                     // 공격 가능 거리
    public Action onAttack;
    public Action onExitAttackState;
    [SerializeField] protected bool isAttacking = false;      // 공격중인지 판단하는 변수
    [SerializeField] protected float curTimeAttackElaped = 1f;      // 공격중인지 판단하는 변수
    [SerializeField] protected float defaultTimeAttackElaped = 1f;      // 공격중인지 판단하는 변수

    [SerializeField] private float curAnimSpeedMultiplier = 0.5f;      // 공격중인지 판단하는 변수

    // 탐색 관련 -------------------------------------------------------------------------------------------
    [Header("Find")]
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
    private PlayerController player = null;

    public float chaseDis = 7.0f;

    [Header("Test V")]
    public float testATKsinceTIme = 0;      //공격거리 판단
    public float testRayYMulti = 0.5f;  //디버그레이 Y위치 
    public float lastX = 0;

    //컴포넌트
    private EnemySensor_Search enemySensor = null;
    protected Rigidbody2D rigid;
    protected Animator animator;
    private SpriteRenderer spriteRenderer;
    private AnimationManager animationManager;

    // UnityEvent Functions--------------------------------------------------------------------------------------------------------
    #region UnityEvent Functions

    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        enemySensor = transform.GetChild(1).GetComponent<EnemySensor_Search>();

        //Enemy의 탐지 범위에 닿으면 위치를 전송하고 state를 chase로 변경
        enemySensor.onSearchSensor += (playerPosition) =>
        {
            lastSeenPosition = playerPosition;
            State = BehaviorState.Chase;
        };

        RefreshPatrol();


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

        player = GameManager.Instance.Player;

        animationManager = GameManager.Instance.AnimationManager;
        animationManager.onAnimSlow += AttackVariableChange;
    }

    private void OnEnable()
    {

    }

    private void Update()
    {
        onUpdate();
        lastX = lastSeenPosition.x;
        testATKsinceTIme = timeSinceAttack;
    }

    #endregion

    #region State Updates

    void Update_Patrol()
    {
        if (isRightPatrol)
        {
            if (transform.position.x < rightPatrol)
            {
                rigid.velocity = new Vector2(curWalkSpeed * facingDirection, rigid.velocity.y);
            }
            else
            {
                FacingDirection = -1;
                isRightPatrol = false;
            }
        }
        else
        {
            if (transform.position.x > leftPatrol)
            {
                rigid.velocity = new Vector2(curWalkSpeed * facingDirection, rigid.velocity.y);
            }
            else
            {
                FacingDirection = 1;
                isRightPatrol = true;
            }
        }

        // 플레이어가 탐색센서에 닿으면 CHase로 변경

    }

    void Update_Chase()
    {
        SetFacingDirection();

        if (IsPlayerInSight(out Vector3 position))
        {
            lastSeenPosition = position; // 플레이어의 마지막 위치 저장
        }

        MoveTowards(lastSeenPosition); // 마지막 위치로 이동

        // 현재 위치의 x가 lastSeenPosition의 x에 거의 도달했는지 확인
        if (Mathf.Abs(transform.position.x - lastSeenPosition.x) <= stoppingDistance)
        {
            // 상태를 Find로 변경
            State = BehaviorState.Find;
        }
    }

    void Update_Find()
    {
        findTimeElapsed += Time.deltaTime;
        if (findTimeElapsed > findTime)
        {
            State = BehaviorState.Patrol;   // 일정 시간이 지날때까지 플레이어를 못찾음 -> 배회 상태로 변경
        }
    }


    void Update_Attack()
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

    void Update_Dead()
    {
        //animator.SetTrigger("Die");
        Destroy(this.gameObject, 2f);
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

    /// <summary>
    /// 특정 샅애에서 나갈때의 처리를 실행하는 함수
    /// </summary>
    /// <param name="oldState">옛 상태</param>
    void OnStateExit(BehaviorState oldState)
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
    /// 플레이어가 시야범위 안에 있는지 확인하는 함수
    /// </summary>
    /// <param name="position">플레이어가 시야범위 안에 있을 때 플레이어의 위치</param>
    /// <returns>true면 시야범위 안에 있다. false면 시야범위 안에 없다.</returns>
    private bool IsPlayerInSight(out Vector3 position)
    {
        position = Vector3.zero;

        // 레이캐스트의 길이 (거리) chaseDis

        // 레이캐스트의 방향 (적의 앞 방향)
        Vector2 direction = transform.right * facingDirection;

        // 디버그 라인을 그리기 위한 시작점과 끝점
        Vector2 startPoint = transform.position + Vector3.up * testRayYMulti;
        Vector2 endPoint = startPoint + direction * chaseDis;

        // 레이어 마스크 설정 (Enemy 레이어를 무시)
        //int layerMask = ~(1 << LayerMask.NameToLayer("Enemy"));

        // 레이어 마스크 설정 (Enemy 레이어와 EnemyAttack 레이어를 무시)
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int enemyAttackLayer = LayerMask.NameToLayer("EnemyAttack");
        int layerMask = ~(1 << enemyLayer | 1 << enemyAttackLayer);

        // 레이캐스트 발사
        RaycastHit2D hit = Physics2D.Raycast(startPoint, direction, chaseDis, layerMask);

        // 디버그 라인 그리기 (색상: 빨강)
        Debug.DrawLine(startPoint, endPoint, Color.red);

        //if(hit.collider != null)
        //{
        //    Debug.Log(hit.collider.gameObject.name);
        //}

        // 레이캐스트가 충돌한 경우
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            Debug.DrawLine(startPoint + Vector2.up * testRayYMulti, endPoint, Color.yellow);
            position = hit.transform.position;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 위치로 이동하는 함수
    /// </summary>
    /// <param name="targetPosition">이동할 위치</param>
    private void MoveTowards(Vector3 targetPosition)
    {
        targetPosition = new Vector3(targetPosition.x, 0, 0);
        Vector3 myPosition = new Vector3(transform.position.x, 0, 0);
        Vector2 direction = (targetPosition - myPosition).normalized;
        rigid.velocity = direction * curRunSpeed;
    }

    /// <summary>
    /// Patrol 할 좌우 X 위치 지정 함수
    /// </summary>
    private void RefreshPatrol()
    {
        leftPatrol = transform.position.x - patrolRange;
        rightPatrol = transform.position.x + patrolRange;
    }

    //State를 Attack으로 변경한다. AttackSensor에서 사용
    public void SetAttackState()
    {
        State = BehaviorState.Attack;
    }

    //공격 시작 함수
    protected virtual void AttackTry()
    {
        //currentAttack++;
        //if (currentAttack > 3)
        //    currentAttack = 1;
        //if (timeSinceAttack > 4.5f)
        //    currentAttack = 1;
        //아직 애니메이션 없어서 주석처리함
        //animator.SetTrigger("Attack" + currentAttack);
        //
        //공격 눌렀다고 알림 = 공격 범위 활성화 
        onAttack?.Invoke();         //DoAttack() 함수로 대신 사용해서 애니메이션 이벤트로 활용



        timeSinceAttack = 0.0f;
        
        animator.SetTrigger("Attack");

        StartCoroutine(Attacking_Physics());
    }

    /// <summary>
    /// 공격 시 물리 적용 코루틴 + timeAttackElaped만큼 기다린 후 isAttacking false로 전환
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator Attacking_Physics()
    {
        isAttacking = true;
        rigid.velocity = new Vector2(attackForce * facingDirection, rigid.velocity.y);
        yield return new WaitForSeconds(curTimeAttackElaped);
        isAttacking = false;
    }

    /// <summary>
    /// 공격 범위 활성화 델리게이트 전송 함수. 애니메이션 이벤트로 사용
    /// </summary>
    protected virtual void DoAttack()
    {
        //공격 눌렀다고 알림 = 공격 범위 활성화
        onAttack?.Invoke();
    }


    /// <summary>
    /// 공격 상태에서 플레이어를 바라보는 함수
    /// </summary>
    private void SetFacingDirection()
    {
        if (transform.position.x < player.transform.position.x)
            FacingDirection = 1;
        else
            FacingDirection = -1;
    }

    /// <summary>
    /// AniamationManager에서 속도을 느리게 하면 공격관련, 이동속도 변수를 느리게 하는 변수
    /// </summary>
    /// <param name="isSlow">델리게이트로 받은 값. true면 느려지고, false면 정상으로 돌아온다</param>
    private void AttackVariableChange(bool isSlow)
    {
        curAnimSpeedMultiplier = GetInverse(animationManager.SpeedMultiplier);
        if (isSlow)
        {
            //딜레이는 늘어나고 속도는 줄어들게 함
            curAttackDelay *= curAnimSpeedMultiplier;
            curTimeAttackElaped *= curAnimSpeedMultiplier;

            curWalkSpeed *= animationManager.SpeedMultiplier;
            curRunSpeed *= animationManager.SpeedMultiplier;
        }
        else
        {
            curAttackDelay = defaultAttackDelay;
            curTimeAttackElaped = defaultTimeAttackElaped;

            curWalkSpeed = defaultWalkSpeed;
            curRunSpeed = defaultRunSpeed;
        }
        
    }

    /// <summary>
    /// 받은 값의 역수를 반환하는 함수
    /// </summary>
    /// <param name="value">float 0.25일때 리턴은 4</param>
    /// <returns>float 0.25일때 리턴은 4</returns>
    public float GetInverse(float value)
    {
        if (value == 0)
        {
            Debug.LogError("0은 역수를 가질 수 없습니다.");
            return float.NaN; // Not a Number를 반환하여 오류 표시
        }

        float result = 1 / value;
        return Mathf.Round(result * 100f) / 100f; // 소수점 두 자리로 반올림
    }

    // 미구현-----------------------------------------------------------------------------
    // ---------------------미구현--------------------------------------------------------
    // ------------------------------------미구현-----------------------------------------

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
