using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySensor : MonoBehaviour
{

    //public Action<GameObject> onSensorTriggered;
    public Action onSensorTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
           // onSensorTriggered?.Invoke(other.gameObject);
        }
    }
}
