using UnityEngine;

public class PlayerExperience : HaiMonoBehaviour
{
    [SerializeField] private int currentExperience = 0;

    public int CurrentExperience => currentExperience;

    protected override void ResetValues()
    {
        base.ResetValues();
        currentExperience = 0;
    }

    public void GainExperience(int amount)
    {
        if (amount <= 0) return;

        currentExperience += amount;
        Debug.Log($"{name}: Gained {amount} XP. Total XP = {currentExperience}", gameObject);
    }
}