using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyController : CharacterBase
{
    public bool TESTisPATERN = false;
    public int TESTpaternNum= 0;
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
        Stun,
        Dead        // 사망상태. 죽는다.(일정 시간 후에 재생성)
    }

    /// <summary>
    /// 적의 현재 상태
    /// </summary>
    [SerializeField] private BehaviorState state = BehaviorState.Dead;

    /// <summary>
    /// 적의 상태 확인 및 설정용 프로퍼티
    /// </summary>
    protected BehaviorState State
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
    protected Action onUpdate = null;

 
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
    [SerializeField] protected bool isRightPatrol = true;


    [Header("Sprite")]
    /// <summary>
    /// 스프라이트가 기본적으로 오른쪽일때 true. facingDirection에 따라 sprite가 flip할때 기준을 정함
    /// </summary>
    public bool isSpriteRight = true;
    [SerializeField] private int facingDirection = 1;                // 캐릭터가 바라보는 방향
    public int FacingDirection
    {
        get => facingDirection;
        protected set
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
    protected Vector3 lastSeenPosition;               

    public float stoppingDistance = 1.8f;          //마지막으로 본 위치에 도달했다고 판정하는 거리

    // 공격 관련 ----------------------++++++++$$$$$$$$$$$+++++=-----------------------------------------------------

    [Header("Attack")]
    public int maxPaternNum = 1;
    public int curPaternNum = 0;

    [SerializeField] protected int currentAttack = 0;         // 현재 공격 단계
    /// <summary>
    /// 마지막 공격 이후 경과 시간
    /// </summary>
    protected float timeSinceAttack = 0.0f;
    /// <summary>
    /// 공격 대기 시간
    /// </summary>
    [SerializeField] protected float curAttackDelay = 1.5f;
    [SerializeField] protected float defaultAttackDelay = 1.5f;      // 공격 대기 시간
    /// <summary>
    ///  공격 시 앞으로 나갈 거리
    /// </summary>
    [SerializeField] protected float attackForce = 2.0f;     
    /// <summary>
    /// 공격가능거리 플레이어와의 거리가 이보다 크면 공격벗어나게 되어있음
    /// </summary>
    protected float attackDistance = 4.0f;                     // 공격 가능 거리

    protected float attackMaintenanceTime = 0.5f;

    /// <summary>
    /// 공격시 공격범위 콜라이더에 보낼 액션
    /// </summary>
    public Action onAttack_Moment;
    public Action<float> onAttack_Continue;
    /// <summary>
    ///  // 공격중인지 판단하는 변수
    /// </summary>
    [SerializeField] protected bool isAttacking = false;     
    /// <summary>
    /// 공격중에 회전을 막을 시간
    /// </summary>
    [SerializeField] protected float curTimeAttackElaped = 1f;      
    [SerializeField] protected float defaultTimeAttackElaped = 1f;      // 기본 회전방지 시간

    [SerializeField] private float curAnimSpeedMultiplier = 0.5f;      // 현재 애니메이션이 느려질 속도, 기본설정이며 AnimationMAnager에서 설정함
    public Action onRedAttack;  // 붉은 공격 이펙트 발생용 액션

    public Action onStun;
    private float stunTime = 2.5f;
    private float curStunTimer = 0;

    /// <summary>
    /// 패턴 중이다
    /// </summary>
    [SerializeField]
    protected bool isPaternOn = false;

    /// <summary>
    /// 공격 패턴
    /// </summary>
    [SerializeField]
    protected Enums.AttackPatern attackPattern = Enums.AttackPatern.Attack_0;
    public Enums.AttackPatern AttackPattern
    {
        get => attackPattern;
        set
        {
            attackPattern = value;
            onPaternChange?.Invoke((int)value);

            AttackAnimationStart();

            //공격 대기 시간 다시 초기화 - 경직으로 끊겼을떄 대비
            timeSinceAttack = 0.0f;
        }
    }


    /// <summary>
    /// EnemyAttackController로 보낼 공격패턴 번호
    /// </summary>
    public Action<int> onPaternChange;

    // 탐색 관련 -------------------------------------------------------------------------------------------
    [Header("Find")]
    /// <summary>
    /// 탐색 상태에서 배회 상태로 돌아가기까지 걸리는 시간
    /// </summary>
    public float findTime = 5.0f;

    /// <summary>
    /// 탐색 진행 시간
    /// </summary>
    protected float findTimeElapsed = 0.0f;

    /// <summary>
    /// 추적 대상
    /// </summary>
    protected PlayerController player = null;

    public float chaseDis = 7.0f;

    [Header("Test V")]
    public float testATKsinceTIme = 0;      //공격거리 판단
    public float testRayYMulti = 0.5f;  //디버그레이 Y위치 
    public float lastX = 0;


    //컴포넌트
    protected readonly int Hit_Hash = Animator.StringToHash("Hit");
    protected readonly int Death_Hash = Animator.StringToHash("Death");
    protected readonly int Ability_Hash = Animator.StringToHash("Ability");
    protected readonly int Attack0_Hash = Animator.StringToHash("Attack0");
    protected readonly int Attack1_Hash = Animator.StringToHash("Attack1");
    protected readonly int Attack2_Hash = Animator.StringToHash("Attack2");
    protected readonly int Attack3_Hash = Animator.StringToHash("Attack3");
    protected readonly int Attack4_Hash = Animator.StringToHash("Attack4");
    protected readonly int Attack5_Hash = Animator.StringToHash("Attack5");
    protected readonly int Attack6_Hash = Animator.StringToHash("Attack6");
    protected readonly int Stun_Hash = Animator.StringToHash("Stun");

    private EnemySensor_Search enemySensor = null;
    protected Rigidbody2D rigid;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    private AnimationManager animationManager;
    protected EnemyAttackController enemyAttackController = null;

    // UnityEvent Functions--------------------------------------------------------------------------------------------------------
    #region UnityEvent Functions

    protected virtual void Awake()
    {
        enemyAttackController = GetComponentInChildren<EnemyAttackController>();
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


    protected virtual void Start()
    {
        State = BehaviorState.Patrol;

        player = GameManager.Instance.Player;

        animationManager = GameManager.Instance.AnimationManager;
        animationManager.onAnimSlow += AttackVariableChange;

        //액션 등록을 상속을 위해 함수로 구분함
        AttackActionRegistering();
    }


    protected virtual void Update()
    {
        onUpdate();
        lastX = lastSeenPosition.x;
        testATKsinceTIme = timeSinceAttack;
    }

    #endregion

    #region State Updates

    protected virtual void Update_Patrol()
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

    protected virtual void Update_Chase()
    {
        SetFacingDirection();

        if (IsPlayerInSight(out Vector3 position))
        {
            lastSeenPosition = position; // 플레이어의 마지막 위치 저장
        }

        MoveTowards(lastSeenPosition); // 마지막 위치로 이동

        // 현재 위치의 x가 lastSeenPosition의 x에 거의 도달했는지 확인
        if (Mathf.Abs(transform.position.x - player.transform.position.x) < attackDistance)
        {
            // 상태를 Find로 변경
            State = BehaviorState.Attack;
        }

        // 현재 위치의 x가 lastSeenPosition의 x에 거의 도달했는지 확인
        if (Mathf.Abs(transform.position.x - lastSeenPosition.x) <= stoppingDistance)
        {
            // 상태를 Find로 변경
            State = BehaviorState.Find;
        }
    }

    protected virtual void Update_Find()
    {
        findTimeElapsed += Time.deltaTime;
        if (findTimeElapsed > findTime)
        {
            State = BehaviorState.Patrol;   // 일정 시간이 지날때까지 플레이어를 못찾음 -> 배회 상태로 변경
        }
    }

    /// <summary>
    /// 공격 할 떄의 Update문
    /// </summary>
    protected virtual void Update_Attack()
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
                //헌재 공격 패턴을 랜덤으로 정한 값으로 넣음
                if(!TESTisPATERN)
                    ChangePatern();
                else
                {
                    TestPatern();
                }
            }
        }
    }

    protected void Update_Dead()
    {
        //animator.SetTrigger("Die");
        Destroy(this.gameObject, 2f);
    }

    /// <summary>
    /// 스턴 시간 동안 멈추고 시간이 지나면 Chase로 변경
    /// </summary>
    protected void Update_Stun()
    {
        rigid.velocity = new Vector3(0, rigid.velocity.y);
        curStunTimer += Time.deltaTime;
        if(curStunTimer > stunTime)
        {
            curStunTimer = 0;
            State = BehaviorState.Chase;
        }
    }

    #endregion



    /// <summary>
    /// 특정 상태가 되었을 때의 처리를 실행하는 함수
    /// </summary>
    /// <param name="newState">새 상태</param>
    protected virtual void OnStateEnter(BehaviorState newState)
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
            case BehaviorState.Stun:
                animator.SetTrigger(Stun_Hash);
                onStun?.Invoke();
                onUpdate = Update_Stun;
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
    protected virtual void OnStateExit(BehaviorState oldState)
    {
        switch (oldState)
        {
            case BehaviorState.Find:
                findTimeElapsed = 0;    //다시 Find로 돌아가을때를 위해 탐색시간 초기화
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

    #region 이동관련


    /// <summary>
    /// 플레이어가 시야범위 안에 있는지 확인하는 함수
    /// </summary>
    /// <param name="position">플레이어가 시야범위 안에 있을 때 플레이어의 위치</param>
    /// <returns>true면 시야범위 안에 있다. false면 시야범위 안에 없다.</returns>
    protected bool IsPlayerInSight(out Vector3 position)
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
    protected void MoveTowards(Vector3 targetPosition)
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
    /// 받은 값의 역수를 반환하는 함수 시간 느려짐에 활용
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

    /// <summary>
    /// 공격 상태에서 플레이어를 바라보는 함수
    /// </summary>
    protected void SetFacingDirection()
    {
        if (transform.position.x < player.transform.position.x)
            FacingDirection = 1;
        else
            FacingDirection = -1;
    }

    #endregion

    /// <summary>
    /// 랜덤 패턴 시행 - 최대 패턴 개수 만큼 랜덤 돌림 - 복잡한 로직을 위해 virtual
    /// </summary>
    /// <returns>attackPatern의 인덱스</returns>
    protected virtual void ChangePatern()
    {
        int randomP = Random.Range(0, maxPaternNum);
        AttackPattern = (Enums.AttackPatern)randomP;
    }

    /// <summary>
    /// 공격 콜라이더에 플레이어 충돌 시 데미지를 넘길 함수 액션에 등록 - enemyAttackController에서 알림
    /// </summary>
    protected virtual void AttackActionRegistering()
    {
        enemyAttackController.onAttack0 += Attack0_Damage;
        enemyAttackController.onAttack1 += Attakc1_Damage;
    }
    /// <summary>
    /// 공격 애니메이션을 실행하는 함수
    /// </summary>
    protected virtual void AttackAnimationStart()
    {
        switch (attackPattern)
        {
            case Enums.AttackPatern.Attack_0: animator.SetTrigger(Attack0_Hash); break;
            case Enums.AttackPatern.Attack_1: animator.SetTrigger(Attack1_Hash); break;
            case Enums.AttackPatern.Attack_2: animator.SetTrigger(Attack2_Hash); break;
            case Enums.AttackPatern.Attack_3: animator.SetTrigger(Attack3_Hash); break;
            case Enums.AttackPatern.Attack_4: animator.SetTrigger(Attack4_Hash); break;
            case Enums.AttackPatern.Attack_5: animator.SetTrigger(Attack5_Hash); break;
            case Enums.AttackPatern.Attack_6: animator.SetTrigger(Attack6_Hash); break;
        }

    }

    /// <summary>
    /// 공격 범위 활성화 델리게이트 전송 함수. 애니메이션 이벤트로 사용
    /// </summary>
    protected virtual void DoAttack_Collider_Moment()
    {
        //공격 눌렀다고 알림 = 공격 범위 활성화
        onAttack_Moment?.Invoke();
    }

    /// <summary>
    /// 공격 범위 활성화 델리게이트 전송 함수. 애니메이션 이벤트로 사용
    /// </summary>
    protected void DoAttack_Collider_Continue()
    {
        //공격 눌렀다고 알림 = 공격 범위 활성화
        onAttack_Continue?.Invoke(attackMaintenanceTime);
    }

    /// <summary>
    /// 공격 시 물리 적용 코루틴 + timeAttackElaped만큼 기다린 후 isAttacking false로 전환
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator Attacking_Physics()
    {
        rigid.velocity = Vector3.zero;
        isAttacking = true;
        float temp = 0;
        while(temp < curTimeAttackElaped)
        {
            temp += Time.deltaTime;
            rigid.velocity = new Vector2(attackForce * facingDirection, rigid.velocity.y);
            yield return null;
        }
        isAttacking = false;
    }

    private void RedEffect()
    {
        onRedAttack?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void EnterStunnedState()
    {
        State = BehaviorState.Stun;
    }
    private void IsPaternOff()
    {
        //패리 가능 상태 되돌리고 패턴 종료 알림
        parryState = Enums.ParryState.None;
        isPaternOn = false;
    }


    #region 공격패턴마다 정해진 데미지 주는 함수 EnemyAttackController에서 실행됨

    protected override void Attack0_Damage(ICombat.IDamage target)
    {
        // 기본 데미지 공격
        Debug.Log("0 attack 했음");

        base.Attack0_Damage(target);
    }

    protected virtual void Attakc1_Damage(ICombat.IDamage target)
    {
        Debug.Log("1 attack 했음");
        target.TakeDamage(attackPower * 0.6f, transform.position.x);
    }

    protected virtual void Attakc2_Damage(ICombat.IDamage target)
    {
        Debug.Log("2 했음");
        target.TakeDamage(attackPower * 4.0f, transform.position.x);
    }

    #endregion


    private void TestPatern()
    {
        AttackPattern = (Enums.AttackPatern)TESTpaternNum;
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
