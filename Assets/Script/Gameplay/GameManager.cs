using System;
using System.Collections;
using ArcadeVP;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public ArcadeVehicleController player;
    public GameplayState gameplayState;
    
    public Action GameOverEvent;

    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject gameOverUI;
    
    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }
        
        Instance = this;

        GameOverEvent += HandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverEvent -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        playerUI.SetActive(false);
        gameOverUI.SetActive(true);
    }

    public void Start()
    {
        SetState(GameplayState.Tracking);
    }

    public void SetState(GameplayState state)
    {
        gameplayState = state;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
