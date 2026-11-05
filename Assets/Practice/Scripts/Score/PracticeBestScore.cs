using System;
using UnityEngine;

public class PracticeBestScore : HaiMonoBehaviour
{
    private const string BestScoreKey = "PracticeBestScore";

    [Header("References")]
    [SerializeField] private PracticeRunScore runScore;
    [SerializeField] private PracticeGameStateController gameStateController;

    private int bestScore;
    private bool hasUnsavedBestScore;

    public event Action<int> BestScoreChanged;

    public int BestScore => bestScore;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadRunScore();
        LoadGameStateController();
    }

    private void LoadRunScore()
    {
        if (runScore != null) return;
        runScore = FindFirstObjectByType<PracticeRunScore>();
    }

    private void LoadGameStateController()
    {
        if (gameStateController != null) return;
        gameStateController = FindFirstObjectByType<PracticeGameStateController>();
    }

    protected override void Awake()
    {
        base.Awake();
        LoadRunScore();
        LoadGameStateController();
        LoadBestScore();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        LoadRunScore();
        LoadGameStateController();
        SubscribeScoreEvents();
        SubscribeGameStateEvents();
    }

    protected override void OnDisable()
    {
        if (hasUnsavedBestScore)
        {
            SaveBestScoreToDisk();
        }

        UnsubscribeScoreEvents();
        UnsubscribeGameStateEvents();
        base.OnDisable();
    }

    protected override void Start()
    {
        base.Start();
        LoadBestScore();
    }

    private void SubscribeScoreEvents()
    {
        if (runScore == null)
        {
            Debug.LogError($"{name}: Run Score reference is missing.", this);
            return;
        }

        runScore.ScoreCalculated -= HandleScoreCalculated;
        runScore.ScoreCalculated += HandleScoreCalculated;
    }

    private void UnsubscribeScoreEvents()
    {
        if (runScore == null) return;
        runScore.ScoreCalculated -= HandleScoreCalculated;
    }

    private void SubscribeGameStateEvents()
    {
        if (gameStateController == null) return;
        gameStateController.StateChanged -= HandleGameStateChanged;
        gameStateController.StateChanged += HandleGameStateChanged;
    }

    private void UnsubscribeGameStateEvents()
    {
        if (gameStateController == null) return;
        gameStateController.StateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(PracticeGameStates newState)
    {
        if (newState == PracticeGameStates.GameOver || newState == PracticeGameStates.Victory)
        {
            if (hasUnsavedBestScore)
            {
                SaveBestScoreToDisk();
            }
        }
    }

    public void EvaluateScore(int candidateScore)
    {
        HandleScoreCalculated(candidateScore);
        if (hasUnsavedBestScore)
        {
            SaveBestScoreToDisk();
        }
    }

    private void HandleScoreCalculated(int finalScore)
    {
        if (finalScore <= bestScore)
        {
            return;
        }

        // Trong lúc đang chiến đấu: chỉ cập nhật kỷ lục trên RAM và phát sự kiện cho HUD (0 Disk I/O)
        bestScore = finalScore;
        hasUnsavedBestScore = true;

        if (PracticeSaveManager.Instance != null)
        {
            PracticeSaveManager.Instance.SetBestScore(bestScore, false);
        }

        BestScoreChanged?.Invoke(bestScore);
    }

    private void LoadBestScore()
    {
        int prefsBest = PlayerPrefs.GetInt(BestScoreKey, 0);
        int jsonBest = PracticeSaveManager.Instance != null
            ? PracticeSaveManager.Instance.GetBestScore()
            : 0;

        bestScore = Mathf.Max(prefsBest, jsonBest);
        hasUnsavedBestScore = false;

        if (PracticeSaveManager.Instance != null && bestScore > jsonBest)
        {
            PracticeSaveManager.Instance.SetBestScore(bestScore, true);
        }

        BestScoreChanged?.Invoke(bestScore);
    }

    private void SaveBestScoreToDisk()
    {
        hasUnsavedBestScore = false;

        PlayerPrefs.SetInt(BestScoreKey, bestScore);
        PlayerPrefs.Save();

        if (PracticeSaveManager.Instance != null)
        {
            PracticeSaveManager.Instance.SetBestScore(bestScore, true);
        }

        Debug.Log($"{name}: New Best Score flushed to disk: {bestScore}", this);
    }
}