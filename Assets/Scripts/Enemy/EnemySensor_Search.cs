using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class EnemySensor_Search : MonoBehaviour
{
    public Action onSensorTriggered;

    private EnemyController enemy;
    private BoxCollider2D attackRangeCol;

    private float offsetX = 0;
    private float offsetY = 0;
    private void Awake()
    {
        enemy = GetComponentInParent<EnemyController>();
        attackRangeCol = GetComponent<BoxCollider2D>();
        offsetX = attackRangeCol.offset.x;
        offsetY = attackRangeCol.offset.y;
    }
    private void Update()
    {
        attackRangeCol.offset = new Vector2(offsetX * enemy.FacingDirection, offsetY);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            onSensorTriggered?.Invoke();
        }
    }

}
