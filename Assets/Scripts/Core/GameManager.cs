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
}
