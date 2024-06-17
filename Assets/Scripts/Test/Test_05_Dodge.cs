using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_05_Dodge : TestBase
{
    AnimationManager manager;
    PlayerController controller;

    public float testmp = 4;

    private void Start()
    {
        manager = GameManager.Instance.AnimationManager;
        controller = GameManager.Instance.Player;
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

    protected override void OnTest4(InputAction.CallbackContext context)
    {
        controller.onMpChange(testmp / 10);
    }
}
