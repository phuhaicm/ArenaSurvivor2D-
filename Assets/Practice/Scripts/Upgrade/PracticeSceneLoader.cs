using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PracticeSceneLoader : HaiMonoBehaviour
{
    [Header("Loading UI")]
    [SerializeField] protected GameObject loadingPanel;
    [SerializeField] protected CanvasGroup loadingCanvasGroup;
    [SerializeField] protected Slider progressSlider;
    [SerializeField] protected TextMeshProUGUI progressText;
    [Header("Loading Setting")]
    [SerializeField, Min(0f)] protected float miniumLoadingDuration = 1f;
    [Header("Fade Setting")]
    [SerializeField, Min(0f)] protected float fadeDuration = 0.35f;
    protected bool isLoading;
    protected bool IsLoading => isLoading;
    protected override void Start()
    {
        base.Start();

        isLoading = false;
        
        InitializeLoadingUI();
    }
    protected void InitializeLoadingUI()
    {
        RefreshLoadingUI(0f);

        if(loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.interactable = false;
            loadingCanvasGroup.blocksRaycasts = false;
        }
        SetLoadingPanelActive(false);
    }
    public bool LoadScene(string sceneName)
    {
        if(isLoading) return false;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError($"{name}: Scene name is empty.", this);
            return false;
        }
        StartCoroutine(LoadSceneRoutine(sceneName));
        return true;
    }
    protected IEnumerator FadeLoadingPanelIn()
    {
        if(loadingCanvasGroup == null)
        {
            yield break;
        }
        loadingCanvasGroup.alpha = 0f;
        loadingCanvasGroup.interactable = true;
        loadingCanvasGroup.blocksRaycasts = true;

        if(fadeDuration <= 0f)
        {
            loadingCanvasGroup.alpha = 1f;
            yield break;
        }
        float elapsedTime = 0f;

        while(elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float fadeProgress = Mathf.Clamp01(elapsedTime / fadeDuration);

            loadingCanvasGroup.alpha = fadeProgress;
            yield return null;
        }
        loadingCanvasGroup.alpha = 1f;
    }
    protected IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;

        Time.timeScale = 1f;
        SetLoadingPanelActive(true);
        RefreshLoadingUI(0f);
        yield return FadeLoadingPanelIn();//chờ fade in hoàn thành sau đó mới tiếp tục load scene

        float loadingStartTime = Time.unscaledTime;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);

        if(loadOperation == null)
        {
            Debug.LogError($"{name}: Cannot load scene {sceneName}", this);
            
            isLoading = false;
            SetLoadingPanelActive(false);
            yield break;
        }
        loadOperation.allowSceneActivation = false;
        while (!loadOperation.isDone)
        {
            float sceneProgress = Mathf.Clamp01(loadOperation.progress / 0.9f);//kẹp hoặc giới hạn 1 giá trị vào 1 khoảng nhất định

            float elapsedLoadingTime = Time.unscaledTime - loadingStartTime;//time.unscaledTime là số giây đã trôi qua kể từ khi game bắt đầu những ko ảnh hưởng bởi time.timescale 
            float timeProgress = 1f;

            if(miniumLoadingDuration > 0f)
            {
                timeProgress = Mathf.Clamp01(elapsedLoadingTime / miniumLoadingDuration);
            }

            float displayProgress = Mathf.Min(sceneProgress, timeProgress);//ta ko muốn UI chạy nhanh hơn điều kiện còn chậm
            

            RefreshLoadingUI(displayProgress);

            bool sceneIsReady = loadOperation.progress >= 0.9f;
            bool miniumTimeComplete = elapsedLoadingTime >= miniumLoadingDuration;

            if(sceneIsReady && miniumTimeComplete)
            {
                RefreshLoadingUI(1f);
                loadOperation.allowSceneActivation = true;
            }
            yield return null;//cái này mà nằm ngoài thì sẽ bị vòng lặp vô hạn
        }
    }
    protected void RefreshLoadingUI(float progress)
    {
        //if(progress == null) return;
        float validProgress = Mathf.Clamp01(progress);

        if(progressSlider != null)
        {
            progressSlider.value = validProgress;
        }
        if(progressText != null)
        {
            int percentage = Mathf.RoundToInt(validProgress * 100f);

            progressText.text = $"Loading ... {percentage}%";
        }
    }
    protected void SetLoadingPanelActive(bool isActive)
    {
        if(loadingPanel == null) return;

        loadingPanel.SetActive(isActive);
    }
}

