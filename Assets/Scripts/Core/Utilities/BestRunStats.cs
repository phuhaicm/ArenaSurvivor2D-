using UnityEngine;

public static class BestRunStats
{
    private const string BestTimeKey = "BestTime";
    private const string BestLevelKey = "BestLevel";
    private const string BestXPKey = "BestXP";

    public static float BestTime => PlayerPrefs.GetFloat(BestTimeKey, 0f);
    public static int BestLevel => PlayerPrefs.GetInt(BestLevelKey, 0);
    public static int BestXP => PlayerPrefs.GetInt(BestXPKey, 0);

    public static void SaveIfBetter(float timeSurvived, int levelReached, int xpCollected)
    {
        if (timeSurvived > BestTime)
        {
            PlayerPrefs.SetFloat(BestTimeKey, timeSurvived);
        }

        if (levelReached > BestLevel)
        {
            PlayerPrefs.SetInt(BestLevelKey, levelReached);
        }

        if (xpCollected > BestXP)
        {
            PlayerPrefs.SetInt(BestXPKey, xpCollected);
        }

        PlayerPrefs.Save();
    }
}