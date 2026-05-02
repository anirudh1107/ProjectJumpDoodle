using System;
using TMPro;
using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    public static MainUIManager Instance { get; private set; }

    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameoverPanel;
    [SerializeField] private TMP_Text gameOverScoreText;


     private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        mainMenuPanel.SetActive(false);
        gameoverPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        gameoverPanel.SetActive(false);
    }

    public void ShowGameoverPanel()
    {
        mainMenuPanel.SetActive(false);
        gameoverPanel.SetActive(true);
    }

    public void PlayGame()
    {
        GameManager.Instance.ChangeState(GameState.GameStart);
    }

    public void RestartGame()
    {
        GameManager.Instance.ChangeState(GameState.GameStart);
    }

    public void HideMainMenuPanel()
    {
        mainMenuPanel.SetActive(false);
    }

    public void HideGameoverPanel()
    {
        gameoverPanel.SetActive(false);
    }

    public void UpdateGameOverScore(int score)
    {
        gameOverScoreText.text = score.ToString("F2");  
    }
}
