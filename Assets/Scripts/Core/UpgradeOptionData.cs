public readonly struct UpgradeOptionData
{
    public PlayerUpgradeType Type { get; }
    public string DisplayName { get; }
    public string Description { get; }

    public UpgradeOptionData(PlayerUpgradeType type, string displayName, string description)
    {
        Type = type;
        DisplayName = displayName;
        Description = description;
    }
}
