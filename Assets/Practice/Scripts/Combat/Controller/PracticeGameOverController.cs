using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PracticeGameOverController : HaiMonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PracticePlayerHealth playerHealth;
    [SerializeField] private PracticePlayerMovement playerMovement;
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    private bool isGameOver;
    
    protected override void Awake()
    {
    base.Awake();

    if (playerHealth == null)
    {
        Debug.LogError(
            $"{name}: Chưa gán Player Health.",
            this
        );
    }

    if (playerMovement == null)
    {
        Debug.LogError(
            $"{name}: Chưa gán Player Movement.",
            this
        );
    }

    if (gameOverPanel == null)
    {
        Debug.LogError(
            $"{name}: Chưa gán Game Over Panel.",
            this
        );
    }

    if (restartButton == null)
    {
        Debug.LogError(
            $"{name}: Chưa gán Restart Button.",
            this
        );
    }
    }
    protected override void Start()
    {
        base.Start();
        isGameOver = false;
        HideGameOver();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribePlayerEvents();
        SubscribeButtonEvents();

    }
    protected void HideGameOver()
    {
        if(gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); //ẩn gameoverpanel
        }
        Time.timeScale = 1f;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        UnSubscribePlayerEvents();
        UnSubscribeButtonEvents();
    }
    protected void SubscribePlayerEvents()
    {
        if(playerHealth == null) return;
        this.playerHealth.Died += this.HandlePlayerDied;
    }
    protected void SubscribeButtonEvents()
    {
        if(restartButton == null) return;
        restartButton.onClick.AddListener(RestartGame);

        if(quitButton == null) return;
        quitButton.onClick.AddListener(QuitGame);
    }
    protected void UnSubscribePlayerEvents()
    {
        if(playerHealth == null) return;
        this.playerHealth.Died -= this.HandlePlayerDied;
    }
    protected void UnSubscribeButtonEvents()
    {
        if(restartButton == null) return;
        restartButton.onClick.RemoveListener(RestartGame);

        if(quitButton == null) return;
        quitButton.onClick.RemoveListener(QuitGame);

    }
    private void HandlePlayerDied()
    {
        if(isGameOver) return;

        isGameOver = true;
        ShowGameOver();
        playerMovement.SetMoveEnable(false);

        Time.timeScale = 0f;
    }
    protected void ShowGameOver()
    {
        if(gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Time.timeScale = 0f;
    }
    private void RestartGame()
    {
        if(!isGameOver) return;
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);//lấy index của scene đang chạy
    }
    protected void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
    
}
