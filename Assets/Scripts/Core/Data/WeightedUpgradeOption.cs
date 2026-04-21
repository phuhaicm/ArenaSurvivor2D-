public readonly struct WeightedUpgradeOption
{
    public UpgradeOptionData Option { get; }
    public int Weight { get; }

    public WeightedUpgradeOption(UpgradeOptionData option, int weight)
    {
        Option = option;
        Weight = weight;
    }
}