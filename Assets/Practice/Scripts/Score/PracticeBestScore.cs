using System;
using UnityEngine;

public class PracticeBestScore : HaiMonoBehaviour
{
    private const string BestScoreKey =
        "PracticeBestScore";

    [Header("References")]
    [SerializeField]
    private PracticeRunScore runScore;

    private int bestScore;

    public event Action<int> BestScoreChanged;

    public int BestScore => bestScore;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadRunScore();
    }

    private void LoadRunScore()
    {
        if (runScore != null)
        {
            return;
        }

        runScore =
            FindFirstObjectByType<PracticeRunScore>();
    }

    protected override void Awake()
    {
        base.Awake();
        LoadRunScore();
        LoadBestScore();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadRunScore();
        SubscribeScoreEvents();
    }

    protected override void OnDisable()
    {
        UnsubscribeScoreEvents();

        base.OnDisable();
    }

    protected override void Start()
    {
        base.Start();

        LoadBestScore();
    }

    // =========================================================
    // RUN SCORE EVENTS
    // =========================================================

    private void SubscribeScoreEvents()
    {
        if (runScore == null)
        {
            Debug.LogError(
                $"{name}: Run Score reference is missing.",
                this
            );

            return;
        }

        runScore.ScoreCalculated -=
            HandleScoreCalculated;

        runScore.ScoreCalculated +=
            HandleScoreCalculated;
    }

    private void UnsubscribeScoreEvents()
    {
        if (runScore == null)
        {
            return;
        }

        runScore.ScoreCalculated -=
            HandleScoreCalculated;
    }

    public void EvaluateScore(int candidateScore)
    {
        HandleScoreCalculated(candidateScore);
    }

    private void HandleScoreCalculated(int finalScore)
    {
        if (finalScore <= bestScore)
        {
            return;
        }

        // Cập nhật dữ liệu nội bộ trước.
        bestScore = finalScore;

        // Sau đó lưu dữ liệu xuống JSON & PlayerPrefs.
        SaveBestScore();

        // Cuối cùng mới thông báo cho subscriber.
        BestScoreChanged?.Invoke(bestScore);
    }

    // =========================================================
    // SAVE / LOAD (JSON + PlayerPrefs Sync)
    // =========================================================

    private void LoadBestScore()
    {
        int prefsBest = PlayerPrefs.GetInt(BestScoreKey, 0);
        int jsonBest = PracticeSaveManager.Instance != null
            ? PracticeSaveManager.Instance.GetBestScore()
            : 0;

        bestScore = Mathf.Max(prefsBest, jsonBest);

        if (PracticeSaveManager.Instance != null && bestScore > jsonBest)
        {
            PracticeSaveManager.Instance.SetBestScore(bestScore, true);
        }

        Debug.Log(
            $"{name}: Best Score loaded: {bestScore}",
            this
        );

        BestScoreChanged?.Invoke(bestScore);
    }

    private void SaveBestScore()
    {
        PlayerPrefs.SetInt(
            BestScoreKey,
            bestScore
        );

        PlayerPrefs.Save();

        if (PracticeSaveManager.Instance != null)
        {
            PracticeSaveManager.Instance.SetBestScore(bestScore, true);
        }

        Debug.Log(
            $"{name}: New Best Score saved: {bestScore}",
            this
        );
    }
}