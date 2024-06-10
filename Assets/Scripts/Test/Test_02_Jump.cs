using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_02_Jump : TestBase
{
    public PlayerController controller;

    public float testForce = 11f;
    protected override void OnTest1(InputAction.CallbackContext context)
    {
        controller.Test_JumpForce(testForce);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        controller.State = Enums.ActiveState.None;
    }
    protected override void OnTest3(InputAction.CallbackContext context)
    {
        controller.State = Enums.ActiveState.Active;
    }
    protected override void OnTest4(InputAction.CallbackContext context)
    {
        controller.State = Enums.ActiveState.NoGravity;
    }
}
