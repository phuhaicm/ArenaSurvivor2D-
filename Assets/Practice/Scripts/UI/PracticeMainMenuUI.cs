using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PracticeMainMenuUI : HaiMonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField]
    private Button playButton;

    [SerializeField]
    private Button quitButton;

    [Header("Scene Loading")]
    [SerializeField]
    private PracticeSceneLoader sceneLoader;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadSceneLoader();
    }

    private void LoadSceneLoader()
    {
        if (sceneLoader != null)
        {
            return;
        }
        sceneLoader = PracticeSceneLoader.Instance;

        if (sceneLoader != null)
        {
            return;
        }

        sceneLoader =
            FindFirstObjectByType<PracticeSceneLoader>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadSceneLoader();
        SanitizeAllButtons();
        SubscribeButtonEvents();
    }

    protected override void OnDisable()
    {
        UnsubscribeButtonEvents();

        base.OnDisable();
    }

    protected override void Start()
    {
        base.Start();

        Time.timeScale = 1f;
    }

    private void SanitizeAllButtons()
    {
        SanitizeButtonChildren(playButton);
        SanitizeButtonChildren(quitButton);
    }

    private void SanitizeButtonChildren(Button parentButton)
    {
        if (parentButton == null) return;

        Button[] childButtons = parentButton.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < childButtons.Length; i++)
        {
            if (childButtons[i] != null && childButtons[i] != parentButton)
            {
                childButtons[i].enabled = false;
            }
        }

        TMP_Text[] childTexts = parentButton.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < childTexts.Length; i++)
        {
            if (childTexts[i] != null)
            {
                childTexts[i].raycastTarget = false;
            }
        }
    }

    // =========================================================
    // BUTTON EVENTS
    // =========================================================

    private void SubscribeButtonEvents()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(
                HandlePlayClicked
            );

            playButton.onClick.AddListener(
                HandlePlayClicked
            );
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(
                HandleQuitClicked
            );

            quitButton.onClick.AddListener(
                HandleQuitClicked
            );
        }
    }

    private void UnsubscribeButtonEvents()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(
                HandlePlayClicked
            );
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(
                HandleQuitClicked
            );
        }
    }

    // =========================================================
    // BUTTON HANDLERS
    // =========================================================

    private void HandlePlayClicked()
    {
        PracticeAudioManager.Instance?.PlayButtonClickSfx();

        if (sceneLoader == null)
        {
            LoadSceneLoader();
        }

        Time.timeScale = 1f;

        if (sceneLoader != null)
        {
            bool loadingStarted =
                sceneLoader.LoadScene(
                    PracticeSceneNames.GamePlay
                );

            if (loadingStarted)
            {
                if (playButton != null)
                {
                    playButton.interactable = false;
                }

                if (quitButton != null)
                {
                    quitButton.interactable = false;
                }
                return;
            }
        }

        SceneManager.LoadScene(PracticeSceneNames.GamePlay);
    }

    private void HandleQuitClicked()
    {
        PracticeAudioManager.Instance?.PlayButtonClickSfx();
        Time.timeScale = 1f;
        Application.Quit();
    }
}
