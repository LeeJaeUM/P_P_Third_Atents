using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBarBase : MonoBehaviour
{
    protected Slider yellowSlider;
    protected Slider redSlider;

    private float downDuration = 0.35f;

    CharacterBase characterBase;
    protected virtual void Awake()
    {
        Transform child = transform.GetChild(0);
        yellowSlider = child.GetComponent<Slider>();

        child = transform.GetChild(1);
        redSlider = child.GetComponent<Slider>();

    }

    protected virtual void Start()
    {
        //액션 등록
        characterBase = GetComponentInParent<CharacterBase>();
        characterBase.onHpChange += UpdateHpBar;
    }

    protected void UpdateHpBar(float percent)
    {
        Debug.Log("액션 발동 됨");
        StartCoroutine(HpBatUpdateCo(percent));
    }

    IEnumerator HpBatUpdateCo(float percent)
    {
        redSlider.value = percent;
        yield return new WaitForSeconds(1.0f);
        float temp = 0;
        float startValue = yellowSlider.value; // 현재 yellowSlider의 값을 저장

        while (temp < downDuration)
        {
            temp += Time.deltaTime;
            float t = temp / downDuration; // 경과 시간을 비율로 계산 (0에서 1 사이의 값)
            yellowSlider.value = Mathf.Lerp(startValue, percent, t); // 선형 보간하여 값 업데이트

            yield return null;
        }

        // 최종 값을 percent로 설정
        yellowSlider.value = percent;
    }

}
