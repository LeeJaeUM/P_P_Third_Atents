using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class EnemyAttackController : EnemySensorBase
{
    //공격범위 안에 들어오는지 확인 하는 스크립트

    private BoxCollider2D[] attackColliders;

    public int curPatern = 0;

    public Action<ICombat.IDamage> onAttack0;
    public Action<ICombat.IDamage> onAttack1;
    public Action<ICombat.IDamage> onAttack2;
    public Action<ICombat.IDamage> onAttack3;
    public Action<ICombat.IDamage> onAttack4;
    public Action<ICombat.IDamage> onAttack5;
    public Action<ICombat.IDamage> onAttack6;
    

    private void Start()
    {
        //공격 범위들 배열에 넣기
        attackColliders = GetComponentsInChildren<BoxCollider2D>();

        //enemyController에서 공격 델리게이트 보내면 공격범위 활성화 : 애니메이션 이벤트 사용 예정
        enemy.onAttack_Moment += () =>
        {
            StartCoroutine(ColliderOnOff_Moment());
        };

        enemy.onAttack_Continue += (duration) =>
        {
            StartCoroutine(ColliderOnOff_Continue(duration));
        };

        enemy.onPaternChange += PaternChange;

        rangeCollider.enabled = true;

    }

    private void PaternChange(int atkIndex)
    {
        curPatern = atkIndex;
        rangeCollider = attackColliders[atkIndex];
    }



    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        //플레이어를 찾았을 때 공격한다. 콜라이더는 enemy로부터 델리게이트를 받아 비/활성화하여 데미지 줌
        // 충돌한 오브젝트에서 IDamage 인터페이스를 얻습니다.
        ICombat.IDamage damageable = collision.GetComponent<ICombat.IDamage>();
        if (damageable != null)
        {
            switch (curPatern)
            {
                case 0:
                    onAttack0?.Invoke(damageable);
                    break;
                case 1:
                    onAttack1?.Invoke(damageable);
                    break;
                case 2:
                    onAttack2?.Invoke(damageable);
                    break;
                case 3:
                    onAttack3?.Invoke(damageable);
                    break;
                case 4:
                    onAttack4?.Invoke(damageable);
                    break;
                case 5:
                    onAttack5?.Invoke(damageable);
                    break;
                case 6:
                    onAttack6?.Invoke(damageable);
                    break;

            }
        }
      
    }
    IEnumerator ColliderOnOff_Moment()
    {
        rangeCollider.enabled = true;
        yield return new WaitForSeconds(0.05f);
        rangeCollider.enabled = false;
    }

    /// <summary>
    /// 지속 시간 동안 계속 공격콜라이더를 활성화 시켜둠
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator ColliderOnOff_Continue(float duration)
    {
        rangeCollider.enabled = true;
        yield return new WaitForSeconds(duration);
        rangeCollider.enabled = false;
    }
}
