using System.Collections;
using TMPro;
using UnityEngine;

public class LevelUpToastController : HaiMonoBehaviour
{
    [SerializeField] private float visibleDuration = 1.2f;

    private PlayerLevelSystem playerLevelSystem;
    private HUDRoot hudRoot;
    private TextMeshProUGUI toastText;
    private Coroutine currentRoutine;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerLevelSystem();
        LoadHUDRoot();
        LoadToastText();
    }

    protected override void Start()
    {
        base.Start();
        HideToastInstant();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribeEvents();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UnsubscribeEvents();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        visibleDuration = 1.2f;
    }

    private void LoadPlayerLevelSystem()
    {
        if (playerLevelSystem != null) return;
        playerLevelSystem = FindFirstObjectByType<PlayerLevelSystem>();
    }

    private void LoadHUDRoot()
    {
        if (hudRoot != null) return;
        hudRoot = UIRootLookup.FindRootInCanvas<HUDRoot>(this);
    }

    private void LoadToastText()
    {
        if (toastText != null) return;

        LevelUpToastTextUI marker = UIRootLookup.FindInRoot<HUDRoot, LevelUpToastTextUI>(this);
        if (marker == null) return;

        toastText = marker.GetComponent<TextMeshProUGUI>();
    }

    private void SubscribeEvents()
    {
        if (playerLevelSystem != null)
        {
            playerLevelSystem.LeveledUp += HandleLeveledUp;
        }
    }

    private void UnsubscribeEvents()
    {
        if (playerLevelSystem != null)
        {
            playerLevelSystem.LeveledUp -= HandleLeveledUp;
        }
    }

    private void HandleLeveledUp(int newLevel)
    {
        if (toastText == null) return;

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(ShowToastRoutine(newLevel));
    }

    private IEnumerator ShowToastRoutine(int level)
    {
        toastText.gameObject.SetActive(true);
        toastText.text = $"LEVEL UP!\nLevel {level}";

        yield return new WaitForSecondsRealtime(visibleDuration);

        toastText.gameObject.SetActive(false);
        currentRoutine = null;
    }

    private void HideToastInstant()
    {
        if (toastText != null)
        {
            toastText.gameObject.SetActive(false);
        }
    }
}