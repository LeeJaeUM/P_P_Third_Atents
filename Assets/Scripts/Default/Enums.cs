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
}
