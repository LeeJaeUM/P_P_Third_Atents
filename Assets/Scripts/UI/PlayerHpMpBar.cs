using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpMpBar : HpBarBase
{

    Slider mpSlider;

    protected override void Awake()
    {
        Transform child = transform.GetChild(1);
        yellowSlider = child.GetComponent<Slider>();

        child = transform.GetChild(2);
        redSlider = child.GetComponent<Slider>();

        child = transform.GetChild(3);
        mpSlider = child.GetComponent<Slider>();
    }

    protected override void Start()
    {
        PlayerController player = GameManager.Instance.Player;
        player.onHpChange += UpdateHpBar;
        player.onMpChange += UpdateMpBar;
    }

    private void UpdateMpBar(float percent)
    {
        mpSlider.value = percent;
    }


}
