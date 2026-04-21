using UnityEngine;

public static class BuildStatsFormatter
{
    public static string FormatTime(float elapsedTime)
    {
        int totalSeconds = Mathf.FloorToInt(elapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return $"{minutes:00}:{seconds:00}";
    }

    public static string FormatFireRate(float shootInterval)
    {
        if (shootInterval <= 0f)
        {
            return "-";
        }

        float shotsPerSecond = 1f / shootInterval;
        return $"{shotsPerSecond:0.00} shots/s";
    }
}