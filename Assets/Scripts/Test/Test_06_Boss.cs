using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_06_Boss : TestBase
{
    public Enemy_Boss boss;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
       boss.TestAttack3();
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        base.OnTest2(context);
    }
}
