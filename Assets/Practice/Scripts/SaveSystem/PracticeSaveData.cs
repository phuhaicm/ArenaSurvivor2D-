using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PracticeSaveData
{
    public const int CurrentSaveVersion = 1;

    [Header("Metadata")]
    public int saveVersion = CurrentSaveVersion;
    public long lastWriteTimestamp;

    [Header("Meta Progression")]
    public int currency;
    public List<string> unlockedContentIds = new List<string>();
    public List<PracticePermanentUpgradeSaveEntry> permanentUpgrades =
        new List<PracticePermanentUpgradeSaveEntry>();

    [Header("Lifetime Statistics")]
    public PracticeStatisticsSaveData statistics =
        new PracticeStatisticsSaveData();

    [Header("Audio & User Settings")]
    public PracticeSettingsSaveData settings =
        new PracticeSettingsSaveData();

    public static PracticeSaveData CreateDefault()
    {
        PracticeSaveData data = new PracticeSaveData
        {
            saveVersion = CurrentSaveVersion,
            lastWriteTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            currency = 0,
            unlockedContentIds = new List<string> { "weapon_default_shooter" },
            permanentUpgrades = new List<PracticePermanentUpgradeSaveEntry>(),
            statistics = new PracticeStatisticsSaveData(),
            settings = new PracticeSettingsSaveData()
        };

        data.ValidateAndClamp();
        return data;
    }

    public void ValidateAndClamp()
    {
        if (saveVersion <= 0)
        {
            saveVersion = CurrentSaveVersion;
        }

        currency = Mathf.Max(0, currency);

        if (unlockedContentIds == null)
        {
            unlockedContentIds = new List<string>();
        }

        if (permanentUpgrades == null)
        {
            permanentUpgrades = new List<PracticePermanentUpgradeSaveEntry>();
        }

        if (statistics == null)
        {
            statistics = new PracticeStatisticsSaveData();
        }
        statistics.ValidateAndClamp();

        if (settings == null)
        {
            settings = new PracticeSettingsSaveData();
        }
        settings.ValidateAndClamp();
    }
}

[Serializable]
public class PracticePermanentUpgradeSaveEntry
{
    public string upgradeId;
    public int level;
}

[Serializable]
public class PracticeStatisticsSaveData
{
    public int totalRuns;
    public int wins;
    public int deaths;
    public float bestSurvivalTime;
    public int bestScore;
    public int totalKills;
    public int bossKills;
    public int highestLevel = 1;
    public float totalPlayTime;

    public void ValidateAndClamp()
    {
        totalRuns = Mathf.Max(0, totalRuns);
        wins = Mathf.Max(0, wins);
        deaths = Mathf.Max(0, deaths);
        bestSurvivalTime = Mathf.Max(0f, bestSurvivalTime);
        bestScore = Mathf.Max(0, bestScore);
        totalKills = Mathf.Max(0, totalKills);
        bossKills = Mathf.Max(0, bossKills);
        highestLevel = Mathf.Max(1, highestLevel);
        totalPlayTime = Mathf.Max(0f, totalPlayTime);
    }
}

[Serializable]
public class PracticeSettingsSaveData//chứa đầy đủ 9 chỉ số thống kê trọn đời
{
    public float masterVolume = 1f;
    public float musicVolume = 0.8f;
    public float sfxVolume = 1f;

    public void ValidateAndClamp()
    {
        masterVolume = Mathf.Clamp01(masterVolume);
        musicVolume = Mathf.Clamp01(musicVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);
    }
}