public readonly struct ExperienceOrbConfig
{
    public string ResourcePath { get; }
    public int ExperienceValue { get; }

    public ExperienceOrbConfig(string resourcePath, int experienceValue)
    {
        ResourcePath = resourcePath;
        ExperienceValue = experienceValue;
    }
}