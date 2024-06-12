using System;
using System.Collections;
using UnityEngine;
using static ICombat;

// PlayerController 클래스는 플레이어 캐릭터의 동작을 제어합니다.
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : CharacterBase
{
    [Header("Move Check")]
    [SerializeField] private float speed = 4.0f;             // 이동 속도
    [SerializeField] private float jumpForce = 7.5f;         // 점프 힘
    [SerializeField] private float rollForce = 6.0f;         // 구르기 힘

    [SerializeField] private bool grounded = false; // 땅에 닿아 있는지 여부
    [SerializeField] private bool isRolling = false; // 구르는 중인지 여부

    //방향처리-------------
    private int facingDirection = 1;                // 캐릭터가 바라보는 방향
    public int FacingDirection => facingDirection;

    [Header("Attack")]
    [SerializeField] private int currentAttack = 0; // 현재 공격 단계
    private float timeSinceAttack = 0.0f;           // 마지막 공격 이후 경과 시간
    private float attackDelay = 0.25f;              // 공격 대기 시간
    private float attackForce = 2.0f;               // 공격 시 앞으로 나갈 거리
    public Action onAttack;

    [Header("Roll")]
    //private float delayToIdle = 0.05f;              // 대기 상태로 전환 대기 시간
    private float rollDuration = 0.25f;             // 구르기 지속 시간
    [SerializeField] private float rollDelay = 1.0f;
    private float timeSinceRoll = 0.0f;             // 구르기 가능 시간 판단 변수

    [Header("Block")]
    [SerializeField] private float blockMultiplier = 0.5f;
    public bool isParryAble = false;
    public bool isBlockAble = false;
    public float parryTime_origin = 2.0f;
    public float parryTime_cur = 0.0f; 
    public Action onParry;  //패리 성공시 발동액션 

    [Header("Gravity")]
    private Vector2 inputDirection = Vector2.zero;  // 입력 방향
    public static float defaultGravityScale = 1f;               //기본 중력 값
    [SerializeField] private float curGravityScale = 1f;          //중력값 확인용
    public bool isFirstCheck = false;


    // 이동제한 및 움직임 스테이트
    [SerializeField] private Enums.ActiveState state = Enums.ActiveState.None;
    public Enums.ActiveState State
    {
        get => state;
        set
        {
            if(state != value)
            {
                state = value;
                switch(value)
                {
                    case Enums.ActiveState.None:
                        rigid.gravityScale = defaultGravityScale;
                        break;
                    case Enums.ActiveState.Active:
                        //rigid.velocity = Vector2.zero;
                        rigid.gravityScale = defaultGravityScale;
                        break;
                    case Enums.ActiveState.NoGravity:
                        rigid.velocity = Vector2.zero;
                        rigid.gravityScale = 0;
                        break;
                }
            }
        }
    }


    // 애니메이터용 해시값들
    #region AnimatorHashs & Components------------___------

    readonly int Jump_Hash = Animator.StringToHash("Jump");
    readonly int AirSpeedY_Hash = Animator.StringToHash("AirSpeedY");
    readonly int IsGround_Hash = Animator.StringToHash("Grounded");
    readonly int Roll_Hash = Animator.StringToHash("Roll");
    readonly int AnimState_Hash = Animator.StringToHash("AnimState");
    readonly int IdleBlock_Hash = Animator.StringToHash("IdleBlock");
    readonly int Parry_Hash = Animator.StringToHash("Parry");

    private Animator animator;                      // 애니메이터
    private Rigidbody2D rigid;                      // 리지드바디
    private PlayerSensor_Ground groundSensor;         // 지면 감지 센서
    private SpriteRenderer spriteRenderer;          // 스프라이트 렌더러
    private PlayerInputHandler inputHandler;        // 플레이어 입력 핸들러
    #endregion

    private void Awake()
    {
        //체력 초기화
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    private void OnEnable()
    {
        inputHandler.OnMove += OnMove;              // 이동 이벤트 등록
        inputHandler.OnStop += OnStop;              // 멈춤 이벤트 등록
        inputHandler.OnJumpPressed += OnJump;        // 점프 이벤트 등록
        inputHandler.OnAttackPressed += OnAttack;    // 공격 이벤트 등록
        inputHandler.OnRollPressed += OnRoll;        // 구르기 이벤트 등록
        inputHandler.OnBlockPressed += OnBlockPerformed;  // 방패 들기 이벤트 등록
        inputHandler.OnBlockReleased += OnBlockCanceled;  // 방패 내리기 이벤트 등록
    }

    private void OnDisable()
    {
        inputHandler.OnMove -= OnMove;              // 이동 이벤트 해제
        inputHandler.OnStop -= OnStop;              // 멈춤 이벤트 해제
        inputHandler.OnJumpPressed -= OnJump;        // 점프 이벤트 해제
        inputHandler.OnAttackPressed -= OnAttack;    // 공격 이벤트 해제
        inputHandler.OnRollPressed -= OnRoll;        // 구르기 이벤트 해제
        inputHandler.OnBlockPressed -= OnBlockPerformed;  // 방패 들기 이벤트 해제
        inputHandler.OnBlockReleased -= OnBlockCanceled;  // 방패 내리기 이벤트 해제
    }

    #region InputActions


    // 방패 내리기 이벤트 처리
    private void OnBlockCanceled()
    {
        isBlockAble = false;
        isParryAble = false;
        animator.SetBool(IdleBlock_Hash, false);
    }

    // 방패 들기 이벤트 처리
    private void OnBlockPerformed()
    {
        if (!isRolling)
        {
            //animator.SetTrigger(Block_Hash);
            isBlockAble = true;
            isParryAble = true;
            animator.SetBool(IdleBlock_Hash, true);
        }
    }

    // 구르기 이벤트 처리
    private void OnRoll()
    {
        if (!isRolling && timeSinceRoll > rollDelay)
        {
            isRolling = true;
            animator.SetTrigger(Roll_Hash);
            State = Enums.ActiveState.NoGravity;
            StartCoroutine(Rolling());
        }
    }

    // 공격 이벤트 처리
    private void OnAttack()
    {
        if (timeSinceAttack > attackDelay && !isRolling)
        {
            currentAttack++;
            if (currentAttack > 3)
                currentAttack = 1;
            if (timeSinceAttack > 1.0f)
                currentAttack = 1;
            animator.SetTrigger("Attack" + currentAttack);
            timeSinceAttack = 0.0f;

            //공격 눌렀다고 알림
            onAttack?.Invoke();

            StartCoroutine(Attacking_Physics());
        }
    }

    // 점프 이벤트 처리
    private void OnJump()
    {
        if (grounded)
        {
            animator.SetTrigger(Jump_Hash);
            grounded = false;
            animator.SetBool(IsGround_Hash, grounded);
            rigid.velocity = new Vector2(rigid.velocity.x, jumpForce);
            groundSensor.Disable(0.2f);
        }
    }

    // 이동 이벤트 처리
    private void OnMove()
    {
        animator.SetInteger(AnimState_Hash, 1);
        inputDirection = inputHandler.InputDirection;

        if (!isRolling)
        {
            //오른쪽을 바라볼때 
            if (inputDirection.x > 0)
            {
                spriteRenderer.flipX = false;
                facingDirection = 1;
            }
            // 왼쪽을 바라볼때
            else if (inputDirection.x < 0)
            {
                spriteRenderer.flipX = true;
                facingDirection = -1;
            }
        }
    }

    // 멈춤 이벤트 처리
    private void OnStop()
    {
        inputDirection = Vector2.zero;
        animator.SetInteger(AnimState_Hash, 0);
    }

    #endregion

    // 시작 초기화
    void Start()
    {
        animator = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        groundSensor = transform.GetChild(0).GetComponent<PlayerSensor_Ground>();
    }

    // 업데이트
    private void Update()
    {
        curGravityScale = rigid.gravityScale;

        //핸들러의 인풋값 받음 // 인풋액션 함수로 이동
        //inputDirection = inputHandler.InputDirection;

        if (!grounded && groundSensor.State())
        {
            grounded = true;
            animator.SetBool(IsGround_Hash, grounded);
        }
        if (grounded && !groundSensor.State())
        {
            grounded = false;
            animator.SetBool(IsGround_Hash, grounded);
        }
        // 점프 시 애니메이션 적용용 float
        animator.SetFloat(AirSpeedY_Hash, rigid.velocity.y);
        
        //공격 딜레이용 시간변수
        timeSinceAttack += Time.deltaTime;

        //구르기 딜레이용 시간변수
        timeSinceRoll += Time.deltaTime;

        //block, parry, hit용 타이머
        ParryTimer();
    }

    //----------------------------- 물리 업데이트-------------------------------------------------------------------------------------__________________---------------------
    private void FixedUpdate()
    {
        switch (state)
        {
            case Enums.ActiveState.None:
                rigid.velocity = new Vector2(inputDirection.x * speed, rigid.velocity.y);
                break;
            case Enums.ActiveState.Active:
                break;
            case Enums.ActiveState.NoGravity:
                break;
            default: // 기본 물리적용
                rigid.velocity = new Vector2(inputDirection.x * speed, rigid.velocity.y);
                break;
        }
        //if (!isRolling)
        //    rigid.velocity = new Vector2(inputDirection.x * speed, rigid.velocity.y);
    }

    // 구르기 코루틴
    IEnumerator Rolling()
    {
        float temp = 0;
        while (temp < rollDuration)
        {
            temp += Time.deltaTime;
            rigid.velocity = new Vector2(facingDirection * rollForce, rigid.velocity.y);
            yield return null;
        }
        isRolling = false;
        if (inputDirection.x > 0)
        {
            spriteRenderer.flipX = false;
            facingDirection = 1;
        }
        else if (inputDirection.x < 0)
        {
            spriteRenderer.flipX = true;
            facingDirection = -1;
        }
        State = Enums.ActiveState.None;
        timeSinceRoll = 0;
    }

    /// <summary>
    /// 공격 시 물리 적용 코루틴 + state 변화
    /// </summary>
    /// <returns></returns>
    IEnumerator Attacking_Physics()
    {
        State = Enums.ActiveState.Active;
        rigid.velocity = new Vector2(attackForce * facingDirection, rigid.velocity.y);
        yield return new WaitForSeconds(attackDelay);
        State = Enums.ActiveState.None;
    }

    public override void Attack(IDamage target)
    {
        base.Attack(target);

        //공격에 성공하면 전진거리의 절반만큼 뒤로 물러남
        rigid.velocity = new Vector2(attackForce * -facingDirection * 0.5f, rigid.velocity.y);
    }

    #region 가드패리 함수 OnBlockInput, ontriggerEnter에서 사용

    /// <summary>
    /// 데미지를 입는 순간에 블록 or 패리 or 피격인지 판단
    /// </summary>
    /// <param name="damage"></param>
    public override void TakeDamage(float damage)
    {
        if (isParryAble)
        {
            onParry?.Invoke();
            animator.SetTrigger(Parry_Hash);
            ParryTimerReset();

            // 패링 시 느려짐 효과 테스트
            StartCoroutine(TimeSlow());

            Debug.Log("패리성공");
        }
        else if (isBlockAble)
        {
            CurrentHealth -= (damage * blockMultiplier);
            BlockComplete();
            Debug.Log("가드로 막음");
        }
        else
        {
            CurrentHealth -= damage;
            ParryTimerReset();
            Debug.Log("그냥 맞아버림");
        }
    }

    void ParryTimer()   //패리 가능한 시간 계산 함수
    {
        if (isParryAble)    //패리가 가능할 때 parryTime_cur은 타이머 처럼 상승
            parryTime_cur += Time.deltaTime;

        if (parryTime_cur > parryTime_origin)    //패리 가능 시간을 넘기면 패리 불가능
            isParryAble = false;
    }

    void ParryTimerReset()  //패리 성공 또는 피격 후 패리 가능 시간 초기화
    {
        parryTime_cur = 0;
        isParryAble = false;
    }

    void BlockComplete()    //가드 성공 시 패리 가능 시간 소폭 연장
    {
        parryTime_cur -= 0.1f;
    }
    #endregion

    private IEnumerator TimeSlow()
    {
        Time.timeScale = 0.6f;
        yield return new WaitForSeconds(0.7f);
        Time.timeScale = 1.0f;
    }

#if UNITY_EDITOR

    public void Test_JumpForce(float force)
    {
        //공중 공격 시 공중에 머무를려면 3이 적당함
        rigid.velocity = new Vector2(rigid.velocity.x, force);
    }

#endif
}
