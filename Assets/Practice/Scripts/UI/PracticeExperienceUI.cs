using UnityEngine;
using UnityEngine.UI;
public class PracticeExperienceUI : HaiMonoBehaviour
{
    [SerializeField] private PracticePlayerLevelSystem playerLevelSystem;
    [SerializeField] private Slider xpSlider;
    
    protected override void OnEnable()
    {
        base.OnEnable();

        if(playerLevelSystem == null) return;
        playerLevelSystem.ExperienceChanged += HandleExperienceChanged;
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        if(playerLevelSystem == null) return;
        playerLevelSystem.ExperienceChanged -= HandleExperienceChanged;
    }
    private void HandleExperienceChanged(float currentXP, float maxXP)
    {
        if(xpSlider == null) return;
        if(maxXP <= 0f)
        {
            xpSlider.value = 0f;
            return;
        }
        xpSlider.value = currentXP / maxXP;
    }
}
