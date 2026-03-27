using UnityEngine;
using System;

public class PlayerExperience : HaiMonoBehaviour
{
    [SerializeField] private int totalExperience = 0;

    public int CurrentExperience => totalExperience;

    public event Action<int> ExperienceGained;

    protected override void ResetValues()
    {
        base.ResetValues();
        totalExperience = 0;
    }

    public void GainExperience(int amount)
    {
        if (amount <= 0) return;

        totalExperience += amount;
        ExperienceGained?.Invoke(amount);

        Debug.Log($"{name}: Gained {amount} XP. Total XP = {totalExperience}", gameObject);
    }
}