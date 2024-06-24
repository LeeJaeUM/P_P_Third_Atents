using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class EnemySensor_Search : EnemySensorBase
{
    public Action<Vector3> onSearchSensor;

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            onSearchSensor?.Invoke(collision.transform.position);
        }
    }
}
