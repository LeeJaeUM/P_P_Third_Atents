using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_02_Jump : TestBase
{
    public PlayerController controller;

    public Rigidbody2D rigidbody2;

    public float testForce = 11f;

    public float testGravity = 0;
    public float testGravityDefault = -15;
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

    protected override void OnTest5(InputAction.CallbackContext context)
    {
        //gravityScale 은 0부터 1사이에 퍼센트를 지정하는거다 -4 해버리면 현재 중력의 4배를 반대 방향으로 적용함
        rigidbody2.gravityScale = testGravity;
    }

    protected override void OnTest6(InputAction.CallbackContext context)
    {
        rigidbody2.gravityScale = testGravityDefault;
    }
}
