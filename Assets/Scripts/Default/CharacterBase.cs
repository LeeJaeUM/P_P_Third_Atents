using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase : MonoBehaviour, ICombat.IDamage, ICombat.IHealth, ICombat.IParryState
{
    //HP관련
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected float currentHealth = 100;
    public virtual float CurrentHealth
    {
        get => currentHealth;
        protected set
        {
            // 최소값은 0, 최대값은 maxHealth로 제한
            currentHealth = Mathf.Clamp(value, 0, maxHealth);

            if (currentHealth <= 0)
            {
                Die(); // HP가 0이하면 사망
            }
        }
    }
    public int MaxHealth => maxHealth;

    /// <summary>
    /// 특수 패리 가능 상태
    /// </summary>
    [SerializeField] protected Enums.ParryState parryState = Enums.ParryState.None;
    public Enums.ParryState ParryState
    {
        get => parryState;
        protected set
        {
            parryState = value;
        }
    }

    /// <summary>
    /// 공격력
    /// </summary>
    [SerializeField] protected float attackPower = 10.0f;

    protected float damageMultiplier = 1.0f;




    // 공격 함수
    protected virtual void Attack0_Damage(ICombat.IDamage target)
    {
        // 공격 로직 구현 // 현재 공격자의 X위치도 포함
        target.TakeDamage(attackPower * damageMultiplier, transform.position.x);
    }

    // 피해 받기 함수
    public virtual void TakeDamage(float damage, float xPos)
    {
        // 피해 로직 구현
        CurrentHealth -= damage;
    }

    /// <summary>
    /// 피격 X위치 판단 함수
    /// </summary>
    /// <param name="facingDir">현재 피격자가 바라보는 방향 1 = 오른쪽, -1 은 왼쪽</param>
    /// <param name="xPos">공격자의 위치</param>
    /// <returns>피격 위치가 정면이면 true, 후방이면 false </returns>
    protected bool HitPosCheck(int facingDir, float xPos)
    {
        if(facingDir == 1)
        {
            if(xPos > transform.position.x)
            {
                return true;
            }
            else
                return false;
        }
        else
        {
            if (xPos > transform.position.x)
            {
                return false;
            }
            else
                return true;
        }
    }

    public virtual void Die()
    {
        Debug.Log($"_{gameObject.name}_이 Die 함");
    }

    /// <summary>
    /// 특수공격이 패리되었을 때 enemy쪽에서 사용될 함수
    /// </summary>
    public virtual void EnterStunnedState()
    {
    }
}
