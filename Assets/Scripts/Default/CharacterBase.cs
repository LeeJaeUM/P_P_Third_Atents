using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase : MonoBehaviour, ICombat.IAttack, ICombat.IDamage, ICombat.IHealth
{
    //HP관련
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected float currentHealth = 100;
    public virtual float CurrentHealth
    {
        get => currentHealth;
        set
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
    /// 공격력
    /// </summary>
    [SerializeField] private float attackPower = 10.0f;

    // 공격 함수
    public void Attack(ICombat.IDamage target)
    {
        // 공격 로직 구현
        target.TakeDamage(attackPower);
    }

    // 피해 받기 함수
    public void TakeDamage(float damage)
    {
        // 피해 로직 구현
        CurrentHealth -= damage;
    }

    public virtual void Die()
    {
        Debug.Log($"_{gameObject.name}_이 Die 함");
    }
}
