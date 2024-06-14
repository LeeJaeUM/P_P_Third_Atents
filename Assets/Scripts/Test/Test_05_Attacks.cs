using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_05_Attacks : TestBase
{
    AnimationManager manager;

    private void Start()
    {
        manager = GameManager.Instance.AnimationManager;
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        manager.TestChangeAnimationSpeed();
    }
    protected override void OnTest2(InputAction.CallbackContext context)
    {
        manager.TestResetAnimationSpeed();
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        manager.TestCOCo();
    }
}
