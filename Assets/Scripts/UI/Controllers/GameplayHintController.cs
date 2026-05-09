using System.Collections;
using TMPro;
using UnityEngine;

public class GameplayHintController : HaiMonoBehaviour
{
    [SerializeField] private float visibleDuration = 5f;

    private GameObject gameplayHintRootObject;
    private GameObject mainMenuRootObject;
    private GameObject hudRootObject;
    private GameObject pauseMenuRootObject;
    private GameObject levelUpPopupRootObject;
    private GameObject gameOverRootObject;

    private TextMeshProUGUI hintText;

    private bool hasShownThisSession;
    private Coroutine hintRoutine;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadGameplayHintRootObject();
        LoadMainMenuRootObject();
        LoadHUDRootObject();
        LoadPauseMenuRootObject();
        LoadLevelUpPopupRootObject();
        LoadGameOverRootObject();
        LoadHintText();
    }

    protected override void Start()
    {
        base.Start();
        HideHintInstant();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        visibleDuration = 5f;
    }

    private void Update()
    {
        if (hasShownThisSession) return;
        if (!CanShowHint()) return;

        ShowGameplayHintOnce();
    }

    private void LoadGameplayHintRootObject()
    {
        if (gameplayHintRootObject != null) return;

        GameplayHintRoot marker = UIRootLookup.FindRootInCanvas<GameplayHintRoot>(this);
        if (marker == null) return;

        gameplayHintRootObject = marker.gameObject;
    }

    private void LoadMainMenuRootObject()
    {
        if (mainMenuRootObject != null) return;

        MainMenuRoot marker = UIRootLookup.FindRootInCanvas<MainMenuRoot>(this);
        if (marker == null) return;

        mainMenuRootObject = marker.gameObject;
    }

    private void LoadHUDRootObject()
    {
        if (hudRootObject != null) return;

        HUDRoot marker = UIRootLookup.FindRootInCanvas<HUDRoot>(this);
        if (marker == null) return;

        hudRootObject = marker.gameObject;
    }

    private void LoadPauseMenuRootObject()
    {
        if (pauseMenuRootObject != null) return;

        PauseMenuRoot marker = UIRootLookup.FindRootInCanvas<PauseMenuRoot>(this);
        if (marker == null) return;

        pauseMenuRootObject = marker.gameObject;
    }

    private void LoadLevelUpPopupRootObject()
    {
        if (levelUpPopupRootObject != null) return;

        LevelUpPopupRoot marker = UIRootLookup.FindRootInCanvas<LevelUpPopupRoot>(this);
        if (marker == null) return;

        levelUpPopupRootObject = marker.gameObject;
    }

    private void LoadGameOverRootObject()
    {
        if (gameOverRootObject != null) return;

        GameOverRoot marker = UIRootLookup.FindRootInCanvas<GameOverRoot>(this);
        if (marker == null) return;

        gameOverRootObject = marker.gameObject;
    }

    private void LoadHintText()
    {
        if (hintText != null) return;

        GameplayHintTextUI marker = UIRootLookup.FindInRoot<GameplayHintRoot, GameplayHintTextUI>(this);
        if (marker == null) return;

        hintText = marker.GetComponent<TextMeshProUGUI>();
    }

    private bool CanShowHint()
    {
        if (gameplayHintRootObject == null || hudRootObject == null) return false;
        if (mainMenuRootObject != null && mainMenuRootObject.activeSelf) return false;
        if (!hudRootObject.activeSelf) return false;
        if (pauseMenuRootObject != null && pauseMenuRootObject.activeSelf) return false;
        if (levelUpPopupRootObject != null && levelUpPopupRootObject.activeSelf) return false;
        if (gameOverRootObject != null && gameOverRootObject.activeSelf) return false;
        return true;
    }

    private void ShowGameplayHintOnce()
    {
        hasShownThisSession = true;

        if (hintText != null)
        {
            hintText.text = "WASD Move  •  ESC Pause  •  TAB Build";
        }

        if (hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
        }

        hintRoutine = StartCoroutine(ShowHintRoutine());
    }

    private IEnumerator ShowHintRoutine()
    {
        if (gameplayHintRootObject != null)
        {
            gameplayHintRootObject.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(visibleDuration);

        if (gameplayHintRootObject != null)
        {
            gameplayHintRootObject.SetActive(false);
        }

        hintRoutine = null;
    }

    private void HideHintInstant()
    {
        if (gameplayHintRootObject != null)
        {
            gameplayHintRootObject.SetActive(false);
        }
    }
}