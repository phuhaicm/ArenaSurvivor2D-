using UnityEngine;
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

        if(sceneLoader != null)
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
        if (sceneLoader == null)
        {
            Debug.LogError(
                $"{name}: PracticeSceneLoader đang null.",
                this
            );

            return;
        }

        Debug.Log(
            $"{name}: Request loading scene " +
            $"{PracticeSceneNames.GamePlay}.",
            this
        );

        bool loadingStarted =
            sceneLoader.LoadScene(
                PracticeSceneNames.GamePlay
            );

        if (!loadingStarted)
        {
            Debug.LogWarning(
                $"{name}: Loading request bị từ chối.",
                this
            );

            return;
        }

        if (playButton != null)
        {
            playButton.interactable = false;
        }

        if (quitButton != null)
        {
            quitButton.interactable = false;
        }
    }

    private void HandleQuitClicked()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}