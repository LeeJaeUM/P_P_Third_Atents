using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICombat
{

    public interface IDamage
    {
        void TakeDamage(float damage, float xPos);
        Enums.ParryState ParryState { get; }
        void ParriedCheck();
    }


    public interface IHealth
    {
        float CurrentHealth { get; }
        int MaxHealth { get; }
        void Die();
    }

}
