using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField]
    private List<Animator> animators = new List<Animator>();
    [SerializeField]
    private Animator playerAnimator;
    [SerializeField]
    private float speedMultiplier = 0.25f;
    public float SpeedMultiplier => speedMultiplier;
    [SerializeField]
    private float timeSlowDuration = 2.0f;

    private bool isSlow = false;

    public Action<bool> onAnimSlow;


    void Start()
    {
        // 플레이어와 다른 오브젝트의 애니메이터를 구분해서 가져오기
        playerAnimator = GameManager.Instance.Player.GetComponent<Animator>();
        Animator[] allAnimators = FindObjectsOfType<Animator>();

        foreach (Animator animator in allAnimators)
        {
            if (animator != playerAnimator)
            {
                animators.Add(animator);
            }
        }
    }
    void ChangeAnimationSpeed()
    {
        isSlow = true;
        onAnimSlow?.Invoke(isSlow);
        foreach (Animator animator in animators)
        {
            animator.speed *= speedMultiplier;
        }
    }

    //원래 애니메이션 속도로 되돌리기
    void ResetAnimationSpeed()
    {
        isSlow = false;
        onAnimSlow?.Invoke(isSlow);
        foreach (Animator animator in animators)
        {
            if (animator != playerAnimator)
            {
                animator.speed = 1.0f;
            }
        }
    }

    private IEnumerator AnimSpeedChange_Co()
    {
        ChangeAnimationSpeed();
        yield return new WaitForSeconds(timeSlowDuration);
        ResetAnimationSpeed();
    }

    public void AnimSlowCo()
    {
        StartCoroutine(AnimSpeedChange_Co());
    }

#if UNITY_EDITOR

    public void TestChangeAnimationSpeed()
    {
        Debug.Log("testStop");
        ChangeAnimationSpeed();
    }

    //원래 애니메이션 속도로 되돌리기
    public void TestResetAnimationSpeed()
    {
        Debug.Log("testDefault");
        ResetAnimationSpeed();
    }

    public void TestCOCo()
    {
        StartCoroutine(AnimSpeedChange_Co());   
    }

#endif
}
