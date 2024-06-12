using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public Collider attackCollider;             // 공격 범위 콜라이더
    public Animator animator;                   // 애니메이터 컴포넌트
    public string attackStateName = "Attack";   // 공격 애니메이션 상태 이름
    public float enablePercent = 0.3f;          // 공격 범위 콜라이더를 활성화할 애니메이션 진행 퍼센트 (0.3 = 30%)
    public float disablePercent = 0.7f;         // 공격 범위 콜라이더를 비활성화할 애니메이션 진행 퍼센트 (0.7 = 70%)

    private bool hasEnabledCollider = false;  // 콜라이더가 이미 활성화되었는지 여부를 추적하는 플래그
    private bool hasDisabledCollider = false; // 콜라이더가 이미 비활성화되었는지 여부를 추적하는 플래그

    void Update()
    {
        // 현재 애니메이터의 상태 정보를 가져옴
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // 현재 애니메이션 상태가 공격 상태인지 확인
        if (stateInfo.IsName(attackStateName))
        {
            // 현재 애니메이션의 진행 퍼센트를 계산 (0.0에서 1.0 사이의 값)
            float normalizedTime = stateInfo.normalizedTime % 1;

            // 애니메이션 진행 퍼센트가 enablePercent 이상이고 아직 콜라이더를 활성화하지 않은 경우
            if (normalizedTime >= enablePercent && !hasEnabledCollider)
            {
                EnableAttackCollider();
            }

            // 애니메이션 진행 퍼센트가 disablePercent 이상이고 아직 콜라이더를 비활성화하지 않은 경우
            if (normalizedTime >= disablePercent && !hasDisabledCollider)
            {
                DisableAttackCollider();
            }
        }
        else
        {
            // 애니메이션 상태가 공격 상태가 아닐 경우, 플래그를 리셋하여 다음 공격에서 다시 사용할 수 있도록 함
            hasEnabledCollider = false;
            hasDisabledCollider = false;
        }
    }

    // 공격 범위 콜라이더를 활성화하는 함수
    void EnableAttackCollider()
    {
        attackCollider.enabled = true;  // 콜라이더 활성화
        hasEnabledCollider = true;  // 활성화 플래그 설정
    }

    // 공격 범위 콜라이더를 비활성화하는 함수
    void DisableAttackCollider()
    {
        attackCollider.enabled = false;  // 콜라이더 비활성화
        hasDisabledCollider = true;  // 비활성화 플래그 설정
    }
}
