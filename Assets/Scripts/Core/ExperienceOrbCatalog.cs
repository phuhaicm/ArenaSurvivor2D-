public static class ExperienceOrbCatalog
{
    public static ExperienceOrbConfig GetConfig(ExperienceOrbSize size)
    {
        switch (size)
        {
            case ExperienceOrbSize.Medium:
                return new ExperienceOrbConfig(GameResourcePaths.XPMediumOrb, 20);

            case ExperienceOrbSize.Large:
                return new ExperienceOrbConfig(GameResourcePaths.XPLargeOrb, 30);

            default:
                return new ExperienceOrbConfig(GameResourcePaths.XPSmallOrb, 10);
        }
    }

    public static ExperienceOrbSize GetSizeFromValue(int xpValue)
    {
        if (xpValue >= 30) return ExperienceOrbSize.Large;
        if (xpValue >= 20) return ExperienceOrbSize.Medium;
        return ExperienceOrbSize.Small;
    }
}