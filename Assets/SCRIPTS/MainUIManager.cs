using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    public static MainUIManager Instance { get; private set; }

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

    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject GameoverPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainMenuPanel.SetActive(false);
        GameoverPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        GameoverPanel.SetActive(false);
    }

    public void ShowGameoverPanel()
    {
        mainMenuPanel.SetActive(false);
        GameoverPanel.SetActive(true);
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
        GameoverPanel.SetActive(false);
    }




}
