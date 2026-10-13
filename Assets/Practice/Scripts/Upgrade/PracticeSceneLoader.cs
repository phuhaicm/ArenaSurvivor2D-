using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PracticeSceneLoader : HaiMonoBehaviour
{
    [Header("Loading UI")]
    [SerializeField] protected GameObject loadingPanel;
    [SerializeField] protected Slider progressSlider;
    [SerializeField] protected TextMeshProUGUI progressText;
    protected bool isLoading;
    protected bool IsLoading => isLoading;
    protected override void Start()
    {
        base.Start();

        isLoading = false;
        SetLoadingPanelActive(false);
        RefreshLoadingUI(0f);
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
    protected IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;

        Time.timeScale = 1f;
        SetLoadingPanelActive(true);
        RefreshLoadingUI(0f);

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
            float normalizedProgress = Mathf.Clamp01(loadOperation.progress / 0.9f);

            RefreshLoadingUI(normalizedProgress);
            if(loadOperation.progress >= 0.9f)
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

