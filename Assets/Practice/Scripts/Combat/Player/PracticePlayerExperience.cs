using System;
using UnityEngine;

public class PracticePlayerExperience : HaiMonoBehaviour
{
    public event Action<float> ExperienceGained;// thông báo nhận bao nhiêu exp     
    public void AddExperience(float amount)
    {
        if (amount <= 0f) return;

        ExperienceGained?.Invoke(amount);
    }
    
}