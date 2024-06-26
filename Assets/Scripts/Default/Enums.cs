using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enums
{
    public enum ActiveState
    {
        Default = 0,
        Active,
        Roll,
        DashAttack,
        NoMoveInput
    }

    public enum ParryState
    {
        None = 0,
        DashAttack,
        SuperGuard
    }

    public enum AttackPatern
    {
        Attack_0,
        Attack_1,
        Attack_2,
        Attack_3,
        Attack_4,
        Attack_5,
        Attack_6
    }

    public enum AttackType
    {
        Moment,
        Continue,
        Dot
    }
}
