using TMPro;
using UnityEngine;

public class PracticeBestScoreUI : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PracticeBestScore bestScoreSystem;

    [Header("UI References")]
    [SerializeField]
    private TextMeshProUGUI bestScoreText;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadBestScoreSystem();
    }

    private void LoadBestScoreSystem()
    {
        if (bestScoreSystem != null)
        {
            return;
        }

        bestScoreSystem =
            FindFirstObjectByType<PracticeBestScore>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadBestScoreSystem();
        SubscribeBestScoreEvents();
    }

    protected override void OnDisable()
    {
        UnsubscribeBestScoreEvents();

        base.OnDisable();
    }

    protected override void Start()
    {
        base.Start();

        RefreshInitialUI();
    }

    // =========================================================
    // EVENTS
    // =========================================================

    private void SubscribeBestScoreEvents()
    {
        if (bestScoreSystem == null)
        {
            Debug.LogError(
                $"{name}: Best Score System reference is missing.",
                this
            );

            return;
        }

        bestScoreSystem.BestScoreChanged -=
            HandleBestScoreChanged;

        bestScoreSystem.BestScoreChanged +=
            HandleBestScoreChanged;

        // Đồng bộ ngay với trạng thái hiện tại. Tại vì nó UI hiển thị nên phải có dòng này để lấy dữ liệu của bên best score
        HandleBestScoreChanged(
            bestScoreSystem.BestScore
        );
    }

    private void UnsubscribeBestScoreEvents()
    {
        if (bestScoreSystem == null)
        {
            return;
        }

        bestScoreSystem.BestScoreChanged -=
            HandleBestScoreChanged;
    }

    // =========================================================
    // UI
    // =========================================================

    private void RefreshInitialUI()
    {
        int currentBestScore = 0;

        if (bestScoreSystem != null)
        {
            currentBestScore =
                bestScoreSystem.BestScore;
        }

        HandleBestScoreChanged(currentBestScore);
    }

    private void HandleBestScoreChanged(int bestScore)
    {
        if (bestScoreText == null)
        {
            Debug.LogError(
                $"{name}: Best Score Text reference is missing.",
                this
            );

            return;
        }

        bestScoreText.text =
            $"Best Score: {bestScore:N0}";
    }
}