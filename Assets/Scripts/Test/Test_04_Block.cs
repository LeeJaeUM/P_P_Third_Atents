using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_04_Block : TestBase
{
    public float curScalse = 0.3f;
    protected override void OnTest1(InputAction.CallbackContext context)
    {
        Time.timeScale = curScalse;
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        Time.timeScale = 1;
    }
}
