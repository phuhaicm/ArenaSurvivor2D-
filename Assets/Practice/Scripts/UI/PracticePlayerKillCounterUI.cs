using TMPro;
using UnityEngine;

public class PracticeKillCounterUI : HaiMonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PracticePlayerKillCounter killCounter;

    [Header("UI References")]
    [SerializeField]
    private TextMeshProUGUI killCountText;

    [SerializeField]
    private TextMeshProUGUI finalKillCountText;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadKillCounter();
    }

    private void LoadKillCounter()
    {
        if (killCounter != null)
        {
            return;
        }

        killCounter =
            FindFirstObjectByType<PracticePlayerKillCounter>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        SubscribeKillCounterEvents();
    }

    protected override void OnDisable()
    {
        UnsubscribeKillCounterEvents();

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

    private void SubscribeKillCounterEvents()
    {
        if (killCounter == null)
        {
            Debug.LogWarning(
                $"{name}: Kill Counter reference is missing.",
                this
            );

            return;
        }

        killCounter.KillCountChanged +=
            HandleKillCountChanged;

        killCounter.KillCountEnded +=
            HandleKillCountingEnded;

        // Đồng bộ dữ liệu hiện tại ngay sau khi subscribe.
        HandleKillCountChanged(
            killCounter.CurrentKills
        );
    }

    private void UnsubscribeKillCounterEvents()
    {
        if (killCounter == null)
        {
            return;
        }

        killCounter.KillCountChanged -=
            HandleKillCountChanged;

        killCounter.KillCountEnded -=
            HandleKillCountingEnded;
    }

    // =========================================================
    // INITIAL UI
    // =========================================================

    private void RefreshInitialUI()
    {
        int currentKills = 0;

        if (killCounter != null)
        {
            currentKills =
                killCounter.CurrentKills;
        }

        HandleKillCountChanged(currentKills);

        if (finalKillCountText != null)
        {
            finalKillCountText.text =
                $"Enemies Defeated: {currentKills}";
        }
    }

    // =========================================================
    // EVENT HANDLERS
    // =========================================================

    private void HandleKillCountChanged(int newKillCount)
    {
        if (killCountText == null)
        {
            return;
        }

        killCountText.text =
            $"Kills: {newKillCount}";
    }

    private void HandleKillCountingEnded(int finalKillCount)
    {
        if (finalKillCountText == null)
        {
            return;
        }

        finalKillCountText.text =
            $"Enemies Defeated: {finalKillCount}";
    }
}