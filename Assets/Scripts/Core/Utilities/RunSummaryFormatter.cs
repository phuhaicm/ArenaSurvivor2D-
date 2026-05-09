using UnityEngine;

public static class RunSummaryFormatter
{
    public static string FormatRunStats(int level, int totalXP, float survivalTime)
    {
        int totalSeconds = Mathf.FloorToInt(survivalTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return
            $"Level Reached: {level}\n" +
            $"XP Collected: {totalXP}\n" +
            $"Time Survived: {minutes:00}:{seconds:00}";
    }

    public static string FormatBestStats()
    {
        int totalSeconds = Mathf.FloorToInt(BestRunStats.BestTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return
            $"Best Time: {minutes:00}:{seconds:00}\n" +
            $"Best Level: {BestRunStats.BestLevel}\n" +
            $"Best XP: {BestRunStats.BestXP}";
    }
}