public static class DifficultyDisplayUtility
{
    public static string GetDangerLabel(float survivalTime)
    {
        if (survivalTime < 30f) return "Low";
        if (survivalTime < 60f) return "Medium";
        if (survivalTime < 90f) return "High";
        return "Extreme";
    }
}
