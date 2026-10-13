using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PracticeMainMenuUI : HaiMonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] protected Button playButton;
    [SerializeField] protected Button quitButton;
    protected override void OnEnable() {
        base.OnEnable();

        SubscribeButtonEvents();
    }
    protected override void OnDisable() 
    {
        base.OnDisable();
        
        UnSubscribeButtonEvents();
    }
    protected override void Start()
    {
        base.Start();

        Time.timeScale = 1f;
    }
    protected void SubscribeButtonEvents()
    {
        if(playButton == null) return;
        playButton.onClick.AddListener(HanldePlayClicked);

        if(quitButton == null) return;
        quitButton.onClick.AddListener(HanldeQuitClicked);
    }
    protected void UnSubscribeButtonEvents()
    {
        if(playButton != null)
        {
            playButton.onClick.RemoveListener(HanldePlayClicked);
        }
        if(quitButton != null)
        {
            quitButton.onClick.RemoveListener(HanldeQuitClicked);
        }
    }
    protected void HanldePlayClicked()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(PracticeSceneNames.GamePlay);
    }
    protected void HanldeQuitClicked()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

}
