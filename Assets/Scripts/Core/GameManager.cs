using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    PlayerController player;
    public PlayerController Player
    {
        get
        {
            if(player == null)
                player = FindAnyObjectByType<PlayerController>();
            return player;
        }
    }

    PlayerInputHandler inputHandler;
    public PlayerInputHandler PlayerInputHandler
    {
        get
        {
            if(inputHandler == null)
                inputHandler = FindAnyObjectByType<PlayerInputHandler>();
            return inputHandler;
        }
    }


    AnimationManager animationManager;
    public AnimationManager AnimationManager
    {
        get
        {
            if(animationManager == null)
                animationManager = FindAnyObjectByType<AnimationManager>();
            return animationManager;
        }
    }

    UIInputHandler uiInputHandler;
    public UIInputHandler UIInputHandler
    {
        get
        {
            if(uiInputHandler == null)
                uiInputHandler = FindAnyObjectByType<UIInputHandler>();
            return uiInputHandler;
        }
    }
}
