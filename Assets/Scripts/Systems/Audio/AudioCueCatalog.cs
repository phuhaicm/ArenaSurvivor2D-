public static class AudioCueCatalog
{
    public static string GetResourcePath(AudioCueKey key)
    {
        switch (key)
        {
            case AudioCueKey.GameplayBgm:
                return "Audio/BGM/GameplayBgm";

            case AudioCueKey.ButtonClick:
                return "Audio/SFX/ButtonClick";

            case AudioCueKey.UpgradePick:
                return "Audio/SFX/UpgradePick";

            case AudioCueKey.GameOver:
                return "Audio/SFX/GameOver";

            default:
                return "Audio/BGM/MenuBgm";
        }
    }
}