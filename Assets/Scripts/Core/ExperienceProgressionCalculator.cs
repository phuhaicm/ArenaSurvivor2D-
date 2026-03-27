using UnityEngine;

public static class ExperienceProgressionCalculator 
{
   public static int GetRequiredExperienceForLevel(int level)
   {
        int normalizedLevel = Mathf.Max(1 , level);
        return 10 + (normalizedLevel - 1) * 5;
   }
}
