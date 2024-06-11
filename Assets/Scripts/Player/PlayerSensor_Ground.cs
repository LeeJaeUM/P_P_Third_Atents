using UnityEngine;
using System.Collections;

public class PlayerSensor_Ground : MonoBehaviour {

    //콜라이더에 무언가 닿으면 +,  나가면 -
    private int m_ColCount = 0;

    private float m_DisableTimer;

    private void OnEnable()
    {
        m_ColCount = 0;
    }

    public bool State()
    {
        //재사용 대기시간 양수일때 점프(행동)을 제한함
        if (m_DisableTimer > 0)
            return false;

        //콜라이더에 무언가 닿고있다면 true, 아무것도 없다면 false
        return m_ColCount > 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        m_ColCount++;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        m_ColCount--;
    }

    void Update()
    {
        //재사용 대기시간 감소
        m_DisableTimer -= Time.deltaTime;
    }

    /// <summary>
    /// 재사용 대기시간용 함수
    /// </summary>
    /// <param name="duration">재사용 대기시간 양수일때 점프(행동)을 제한함</param>
    public void Disable(float duration)
    {
        m_DisableTimer = duration;
    }
}
