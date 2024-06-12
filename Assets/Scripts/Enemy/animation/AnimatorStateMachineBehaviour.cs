using UnityEngine;

public class AnimatorStateMachineBehaviour : StateMachineBehaviour
{
    // 상태 진입 이벤트 핸들러
    public delegate void StateChangeHandler(string stateName);
    public static event StateChangeHandler OnStateEnterEvent;

    // 상태 퇴장 이벤트 핸들러
    public static event StateChangeHandler OnStateExitEvent;

    // Animator 상태 진입 시 호출되는 메서드
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // OnStateEnterEvent 이벤트가 null이 아닐 경우 이벤트를 호출하고 상태 이름을 전달합니다.
        OnStateEnterEvent?.Invoke(stateInfo.shortNameHash.ToString());
    }

    // Animator 상태 퇴장 시 호출되는 메서드
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // OnStateExitEvent 이벤트가 null이 아닐 경우 이벤트를 호출하고 상태 이름을 전달합니다.
        OnStateExitEvent?.Invoke(stateInfo.shortNameHash.ToString());
    }
}
