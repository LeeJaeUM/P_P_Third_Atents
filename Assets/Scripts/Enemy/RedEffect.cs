using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedEffect : MonoBehaviour
{
    private Animator animator;
    private EnemyController enemyController;
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Vector3 curVec = Vector3.zero;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        curVec = transform.localPosition;
    }

    private void Start()
    {
        enemyController = GetComponentInParent<EnemyController>();
        enemyController.onRedAttack += RedAttack;
        spriteRenderer.enabled = false;
    }

    private void RedAttack()
    {
        StartCoroutine(RedEffectCo());
    }

    private IEnumerator RedEffectCo()
    {
        spriteRenderer.enabled = true;
        //이펙트의 좌우 위치 조절
        transform.localPosition = new Vector3(curVec.x * enemyController.FacingDirection, curVec.y);
        animator.SetTrigger("OnEffect");
        yield return new WaitForSeconds(0.6f);
        spriteRenderer.enabled = false;
    }
}
