using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICombat
{
    public interface IAttack
    {
        void Attack(IDamage target);
    }

    public interface IDamage
    {
        void TakeDamage(int damage);
    }
}
