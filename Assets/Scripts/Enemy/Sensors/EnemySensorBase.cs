using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySensorBase : MonoBehaviour
{
    protected EnemyController enemy;
    protected BoxCollider2D rangeCollider;

    protected float offsetX = 0;
    protected float offsetY = 0;
    private void Awake()
    {
        enemy = GetComponentInParent<EnemyController>();
        rangeCollider = GetComponentInChildren<BoxCollider2D>();
        offsetX = rangeCollider.offset.x;
        offsetY = rangeCollider.offset.y;
    }
    protected virtual void Update()
    {
        rangeCollider.offset = new Vector2(offsetX * enemy.FacingDirection, offsetY);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        //if (collision.CompareTag("Player"))
        //{
           
        //}
    }
}
