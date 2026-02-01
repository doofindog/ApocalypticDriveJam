using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public GameplayState gameplayState;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }
        
        Instance = this;
    }

    public void Start()
    {
        SetState(GameplayState.Tracking);
    }

    public void SetState(GameplayState state)
    {
        gameplayState = state;
    }
}
