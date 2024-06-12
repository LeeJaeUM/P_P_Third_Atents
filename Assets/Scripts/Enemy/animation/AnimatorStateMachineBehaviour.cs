using UnityEngine;

public class AnimatorStateMachineBehaviour : StateMachineBehaviour
{
    public delegate void StateChangeHandler(string stateName);
    public static event StateChangeHandler OnStateEnterEvent;
    public static event StateChangeHandler OnStateExitEvent;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        OnStateEnterEvent?.Invoke(stateInfo.shortNameHash.ToString());
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        OnStateExitEvent?.Invoke(stateInfo.shortNameHash.ToString());
    }
}
