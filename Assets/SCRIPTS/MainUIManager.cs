using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{
    public static MainUIManager Instance { get; private set; }

    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameoverPanel;
    [SerializeField] private GameObject HUDPanel;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text gameOverScoreText;
    [SerializeField] private TMP_Text gameOverSecondaryText;
    [SerializeField] private Image healthPoint1;
    [SerializeField] private Image healthPoint2;
    [SerializeField] private Image healthPoint3;


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
        HUDPanel.SetActive(false);
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

    public void ShowHud()
    {
        HUDPanel.SetActive(true);
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

    public void HideHud()
    {
        HUDPanel.SetActive(false);
    }

    public void UpdateGameOverScore(int score)
    {
        gameOverScoreText.text = score.ToString("F2");  
        if(score < 100)
        {
            gameOverSecondaryText.text = "You almost reached the sky Island, try again!";
        }
        else
        {
            gameOverSecondaryText.text = "You almost reached the moon";
        }
    }

    public void UpdateScore(int score)
    {
        scoreText.text = score.ToString("F2");
    }

    public void UpdateHealthDisplay(float currentHealth)
    {
        healthPoint1.enabled = currentHealth >= 1;
        healthPoint2.enabled = currentHealth >= 2;
        healthPoint3.enabled = currentHealth >= 3;
    }
}
