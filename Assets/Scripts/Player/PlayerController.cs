using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static Unity.Collections.AllocatorManager;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 4.0f;
    [SerializeField] float jumpForce = 7.5f;
    [SerializeField] float rollForce = 6.0f;

    private Animator animator;
    private Rigidbody2D rigid;
    [SerializeField]
    private bool grounded = false;
    [SerializeField]
    private bool isRolling = false;

    //구르기에 사용할 방향용 변수
    private int facingDirection = 1;
    [SerializeField]
    private int currentAttack = 0;
    private float timeSinceAttack = 0.0f;
    private float attackDelay = 0.25f;
    private float delayToIdle = 0.05f;
    private float rollDuration = 0.25f;
    private float rollCurrentTime;

    private Vector2 inputDirection = Vector2.zero;

    // 애니메이터용 해시값들
    //readonly int InputX_Hash = Animator.StringToHash("InputX");
    //readonly int InputY_Hash = Animator.StringToHash("InputY");
    //readonly int IsMove_Hash = Animator.StringToHash("IsMove");
    readonly int Jump_Hash = Animator.StringToHash("Jump");
    readonly int AirSpeedY_Hash = Animator.StringToHash("AirSpeedY");
    //readonly int Attack_Hash = Animator.StringToHash("Attack");
    readonly int IsGround_Hash = Animator.StringToHash("Grounded");
    readonly int Roll_Hash = Animator.StringToHash("Roll");
    readonly int Block_Hash = Animator.StringToHash("Block");
    readonly int IdleBlock_Hash = Animator.StringToHash("IdleBlock");

    private Sensor_HeroKnight groundSensor;
    private SpriteRenderer spriteRenderer;
    PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnStop;
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Attack.performed += OnAttack;
        inputActions.Player.Roll.performed += OnRoll;
        inputActions.Player.Block.performed += OnBlockPerformed;
        inputActions.Player.Block.canceled += OnBlockCanceled;
    }

    private void OnDisable()
    {
        inputActions.Player.Block.canceled -= OnBlockCanceled;
        inputActions.Player.Block.performed -= OnBlockPerformed;
        inputActions.Player.Roll.performed -= OnRoll;
        inputActions.Player.Attack.performed -= OnAttack;
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Move.canceled -= OnStop;
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Disable();
    }

    private void OnBlockCanceled(InputAction.CallbackContext context)
    {
        animator.SetBool(IdleBlock_Hash, false);
    }

    private void OnBlockPerformed(InputAction.CallbackContext context)
    {
        if (!isRolling)
        {
            animator.SetTrigger(Block_Hash);
            animator.SetBool(IdleBlock_Hash, true);
        }
    }

    private void OnRoll(InputAction.CallbackContext context)
    {
        if (!isRolling)
        {
            isRolling = true;
            animator.SetTrigger(Roll_Hash);
            StartCoroutine(Rolling());
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        //공격 재사용 대기시간보다 시간이 지났으면
        if (timeSinceAttack > attackDelay && !isRolling)
        {
            currentAttack++;

            // Loop back to one after third attack 최대 콤보 이후 콤보 수 초기화
            if (currentAttack > 3)
                currentAttack = 1;

            // Reset Attack combo if time since last attack is too large 콤보가능시간 지나면 콤보 수 초기화
            if (timeSinceAttack > 1.0f)
                currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            animator.SetTrigger("Attack" + currentAttack);

            // Reset timer
            timeSinceAttack = 0.0f;

        }
    }
    private void OnJump(InputAction.CallbackContext context)
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


    private void OnStop(InputAction.CallbackContext context)
    {

        // 이동 방향을 0으로 만들고
        inputDirection = Vector2.zero;

        // Idle애니메이션을 자연스럽게 이어지게 하기 위해

        // Reset timer
        //StartCoroutine(DelayRunToIdle());
        //적은 시간이여서 코루틴 사용 안하고 바로 멈추도록 함
        animator.SetInteger("AnimState", 0);
    }    
    
    //IEnumerator DelayRunToIdle()
    //{
    //    yield return new WaitForSeconds(delayToIdle);
    //    animator.SetInteger("AnimState", 0);
    //}

    private void OnMove(InputAction.CallbackContext context)
    {
        animator.SetInteger("AnimState", 1);
        // 입력값 받아와서
        inputDirection = context.ReadValue<Vector2>();

        // 애니메이션 조정
        //animator.SetFloat(InputX_Hash, inputDirection.x);
        //animator.SetFloat(InputY_Hash, inputDirection.y);

        // Swap direction of sprite depending on walk direction
        if(!isRolling)
        {
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
        }

    }

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        groundSensor = transform.GetChild(0).GetComponent<Sensor_HeroKnight>();
    }

    private void Update()
    {

        //Check if character just landed on the ground
        if (!grounded && groundSensor.State())
        {
            grounded = true;
            animator.SetBool(IsGround_Hash, grounded);
        }

        //Check if character just started falling
        if (grounded && !groundSensor.State())
        {
            grounded = false;
            animator.SetBool(IsGround_Hash, grounded);
        }

        //Set AirSpeed in animator
        animator.SetFloat(AirSpeedY_Hash, rigid.velocity.y);

        // 어택콤보를 이어가기위한 증가하는 지속시간 확인용 변수
        timeSinceAttack += Time.deltaTime;


    }

    private void FixedUpdate()
    {
        // Move
        if (!isRolling)
            rigid.velocity = new Vector2(inputDirection.x * speed, rigid.velocity.y);
    }

    IEnumerator Rolling()
    {
        float temp = 0;
        while( temp < rollDuration)
        {
            temp += Time.deltaTime;
            rigid.velocity = new Vector2(facingDirection * rollForce, rigid.velocity.y);
            yield return null;
        }

        isRolling = false;

        //Roll 종료 후 Roll중 입력중인 방향으로 플레이어 스프라이트 플립
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
    }


}
