using TMPro;
using UnityEngine;

public class PracticeRunScoreUI : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PracticeRunScore runScore;

    [Header("UI References")]
    [SerializeField]
    private TextMeshProUGUI finalScoreText;

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

    protected override void OnEnable()
    {
        base.OnEnable();

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

        RefreshInitialUI();
    }

    // =========================================================
    // SCORE EVENT
    // =========================================================

    private void SubscribeScoreEvents()
    {
        if (runScore == null)
        {
            Debug.LogWarning(
                $"{name}: Run Score reference is missing.",
                this
            );

            return;
        }

        runScore.ScoreCalculated +=
            HandleScoreCalculated;

        // Đồng bộ với dữ liệu hiện tại.
        HandleScoreCalculated(
            runScore.FinalScore
        );
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

    // =========================================================
    // UI
    // =========================================================

    private void RefreshInitialUI()
    {
        int currentScore = 0;

        if (runScore != null)
        {
            currentScore =
                runScore.FinalScore;
        }

        HandleScoreCalculated(currentScore);
    }

    private void HandleScoreCalculated(int score)
    {
        if (finalScoreText == null)
        {
            return;
        }

        finalScoreText.text =
            $"Final Score: {score:N0}";
    }
}