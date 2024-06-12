using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private List<Animator> animators = new List<Animator>();
    private Animator playerAnimator;

    void Start()
    {
        // 플레이어와 다른 오브젝트의 애니메이터를 구분해서 가져오기
        playerAnimator = GameObject.FindWithTag("Player").GetComponent<Animator>();
        Animator[] allAnimators = FindObjectsOfType<Animator>();

        foreach (Animator animator in allAnimators)
        {
            if (animator != playerAnimator)
            {
                animators.Add(animator);
            }
        }
    }
    void Update()
    {
        if (CheckCondition())
        {
            ChangeAnimationSpeed(0.5f);
        }
        else
        {
            ChangeAnimationSpeed(1.0f);
        }
    }

    bool CheckCondition()
    {
        // 조건 체크 로직 구현 (예: 플레이어가 특정 아이템을 획득했는지 여부 등)
        return true; // 조건이 만족되면 true 반환
    }

    void ChangeAnimationSpeed(float speedMultiplier)
    {
        foreach (Animator animator in animators)
        {
            animator.speed *= speedMultiplier;
        }
    }

    //원래 애니메이션 속도로 되돌리기
    void ResetAnimationSpeed()
    {
        foreach (Animator animator in animators)
        {
            if (animator != playerAnimator)
            {
                animator.speed = 1.0f;
            }
        }
    }
}
