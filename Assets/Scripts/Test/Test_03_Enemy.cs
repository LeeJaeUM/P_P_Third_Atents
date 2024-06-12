using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_03_Enemy : MonoBehaviour
{
    private bool IsPlayerInSight(out Vector3 position)
    {
        position = Vector3.zero;

        // 레이캐스트의 길이 (거리)
        float detectionDistance = 5.0f;

        // 레이캐스트의 방향 (적의 앞 방향)
        Vector2 direction = transform.right;

        // 디버그 라인을 그리기 위한 시작점과 끝점
        Vector2 startPoint = transform.position;
        Vector2 endPoint = startPoint + direction * detectionDistance;

        // 레이캐스트 발사
        RaycastHit2D hit = Physics2D.Raycast(startPoint, direction, detectionDistance);

        // 디버그 라인 그리기 (색상: 빨강)
        Debug.DrawLine(startPoint, endPoint, Color.red);

        // 레이캐스트가 충돌한 경우
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            position = hit.transform.position;
            return true;
        }

        return false;
    }

    private void Update()
    {
        Vector3 playerPosition;
        bool playerInSight = IsPlayerInSight(out playerPosition);

        if (playerInSight)
        {
            Debug.Log("Player detected at position: " + playerPosition);
        }
        else
        {
            Debug.Log("Player not detected.");
        }
    }
}
