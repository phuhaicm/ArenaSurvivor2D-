using System;
using System.IO;
using UnityEngine;

public class PracticeSaveManager : HaiMonoBehaviour
{
    public static PracticeSaveManager Instance { get; private set; }

    private const string SaveFileName = "practice_save_data.json";
    private const string TempFileName = "practice_save_data.json.tmp";
    private const string BackupFileName = "practice_save_data.json.bak";

    // Khớp chính xác với các Key PlayerPrefs cũ trong PracticeBestScore.cs và PracticeAudioSettings.cs
    private const string LegacyBestScoreKey = "PracticeBestScore";
    private const string LegacyMasterVolumeKey = "PracticeMasterVolume";
    private const string LegacyMusicVolumeKey = "PracticeMusicVolume";
    private const string LegacySfxVolumeKey = "PracticeSfxVolume";

    [Header("Persistence Settings")]
    [SerializeField]
    private bool persistAcrossScenes = true;

    [SerializeField]
    private bool prettyPrintJson = true;

    [Header("Runtime Save Data (Read-Only)")]
    [SerializeField]
    private PracticeSaveData currentSaveData;

    public PracticeSaveData Data
    {
        get
        {
            if (currentSaveData == null)
            {
                LoadSaveData();
            }

            return currentSaveData;
        }
    }

    public static string MainSaveFilePath =>
        Path.Combine(Application.persistentDataPath, SaveFileName);

    public static string TempSaveFilePath =>
        Path.Combine(Application.persistentDataPath, TempFileName);

    public static string BackupSaveFilePath =>
        Path.Combine(Application.persistentDataPath, BackupFileName);

    public event Action<PracticeSaveData> SaveDataLoaded;
    public event Action<PracticeSaveData> SaveDataSaved;

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        if (persistAcrossScenes)
        {
            Transform rootTransform = transform.root != null ? transform.root : transform;
            DontDestroyOnLoad(rootTransform.gameObject);
        }

        base.Awake();
        LoadSaveData();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void LoadSaveData()
    {
        // Tầng 1: Thử đọc file JSON chính
        if (TryReadAndValidateJsonFile(MainSaveFilePath, out PracticeSaveData mainData))
        {
            currentSaveData = MigrateSaveDataIfNeeded(mainData);
            SyncLegacyPlayerPrefsBestScore(currentSaveData);
            SaveDataLoaded?.Invoke(currentSaveData);
            return;
        }

        // Tầng 2 (Corrupted Save Recovery): Nếu file chính bị hỏng, tự động khôi phục từ file dự phòng (.bak)
        if (TryReadAndValidateJsonFile(BackupSaveFilePath, out PracticeSaveData backupData))
        {
            Debug.LogWarning(
                "[PracticeSaveManager] File save chính bị hỏng hoặc thiếu. Đã tự động khôi phục từ file Backup (.bak)!"
            );

            currentSaveData = MigrateSaveDataIfNeeded(backupData);
            SaveToDisk();
            SaveDataLoaded?.Invoke(currentSaveData);
            return;
        }

        // Tầng 3: Khởi tạo file Save mặc định và kế thừa dữ liệu PlayerPrefs cũ (BestScore 22,000 + Volume)
        currentSaveData = PracticeSaveData.CreateDefault();
        MigrateFromPlayerPrefs(currentSaveData);
        SaveToDisk();

        SaveDataLoaded?.Invoke(currentSaveData);
    }

    public bool SaveToDisk()
    {
        if (currentSaveData == null)
        {
            currentSaveData = PracticeSaveData.CreateDefault();
        }

        currentSaveData.ValidateAndClamp();
        currentSaveData.saveVersion = PracticeSaveData.CurrentSaveVersion;
        currentSaveData.lastWriteTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        try
        {
            string folderPath = Application.persistentDataPath;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string json = JsonUtility.ToJson(currentSaveData, prettyPrintJson);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError("[PracticeSaveManager] Chuỗi JSON rỗng, hủy thao tác lưu.");
                return false;
            }

            // BƯỚC 1 ATOMIC SAVE: Ghi ra file tạm (.tmp) trước để nếu mất điện giữa chừng không hỏng file chính
            File.WriteAllText(TempSaveFilePath, json);

            FileInfo tempInfo = new FileInfo(TempSaveFilePath);
            if (!tempInfo.Exists || tempInfo.Length == 0)
            {
                Debug.LogError("[PracticeSaveManager] Không thể ghi file tạm (.tmp).");
                return false;
            }

            // BƯỚC 2 ATOMIC SAVE: Sao lưu file chính hiện tại sang file .bak
            if (File.Exists(MainSaveFilePath))
            {
                File.Copy(MainSaveFilePath, BackupSaveFilePath, true);
            }

            // BƯỚC 3 ATOMIC SAVE: Ghi đè file .tmp đã hoàn chỉnh sang file chính (.json) và xóa file .tmp
            File.Copy(TempSaveFilePath, MainSaveFilePath, true);
            File.Delete(TempSaveFilePath);

            SaveDataSaved?.Invoke(currentSaveData);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PracticeSaveManager] Lỗi khi ghi file JSON: {ex.Message}");
            return false;
        }
    }

    public int GetBestScore()
    {
        return Data.statistics.bestScore;
    }

    public void SetBestScore(int newBestScore, bool saveImmediately = true)
    {
        int clampedScore = Mathf.Max(0, newBestScore);

        if (clampedScore > Data.statistics.bestScore)
        {
            Data.statistics.bestScore = clampedScore;

            if (saveImmediately)
            {
                SaveToDisk();
            }
        }
    }

    public void RecordCompletedRun(
        bool isVictory,
        float survivalTime,
        int kills,
        int finalScore,
        int highestLevelReached
    )
    {
        PracticeStatisticsSaveData stats = Data.statistics;

        stats.totalRuns++;

        if (isVictory)
        {
            stats.wins++;
            stats.bossKills++;
        }
        else
        {
            stats.deaths++;
        }

        float validTime = Mathf.Max(0f, survivalTime);
        int validKills = Mathf.Max(0, kills);
        int validScore = Mathf.Max(0, finalScore);
        int validLevel = Mathf.Max(1, highestLevelReached);

        stats.totalPlayTime += validTime;
        stats.totalKills += validKills;

        if (validTime > stats.bestSurvivalTime)
        {
            stats.bestSurvivalTime = validTime;
        }

        if (validScore > stats.bestScore)
        {
            stats.bestScore = validScore;
        }

        if (validLevel > stats.highestLevel)
        {
            stats.highestLevel = validLevel;
        }

        // Thưởng tiền tích lũy (Currency) sau mỗi trận
        Data.currency += Mathf.Max(1, validScore / 10);

        // Chỉ ghi đĩa đúng 1 lần khi kết thúc Run
        SaveToDisk();
    }

    public void SaveVolumeSettings(float master, float music, float sfx)
    {
        Data.settings.masterVolume = Mathf.Clamp01(master);
        Data.settings.musicVolume = Mathf.Clamp01(music);
        Data.settings.sfxVolume = Mathf.Clamp01(sfx);

        SaveToDisk();
    }

    private bool TryReadAndValidateJsonFile(string path, out PracticeSaveData loadedData)
    {
        loadedData = null;

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return false;
        }

        try
        {
            string rawJson = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return false;
            }

            loadedData = JsonUtility.FromJson<PracticeSaveData>(rawJson);
            if (loadedData == null)
            {
                return false;
            }

            loadedData.ValidateAndClamp();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PracticeSaveManager] File '{path}' không hợp lệ: {ex.Message}");
            loadedData = null;
            return false;
        }
    }

    private PracticeSaveData MigrateSaveDataIfNeeded(PracticeSaveData loadedData)
    {
        if (loadedData == null)
        {
            return PracticeSaveData.CreateDefault();
        }

        if (loadedData.saveVersion < PracticeSaveData.CurrentSaveVersion)
        {
            loadedData.saveVersion = PracticeSaveData.CurrentSaveVersion;
        }

        loadedData.ValidateAndClamp();
        return loadedData;
    }

    private void MigrateFromPlayerPrefs(PracticeSaveData targetData)
    {
        SyncLegacyPlayerPrefsBestScore(targetData);

        if (PlayerPrefs.HasKey(LegacyMasterVolumeKey))
        {
            targetData.settings.masterVolume =
                PlayerPrefs.GetFloat(LegacyMasterVolumeKey, 1f);
        }

        if (PlayerPrefs.HasKey(LegacyMusicVolumeKey))
        {
            targetData.settings.musicVolume =
                PlayerPrefs.GetFloat(LegacyMusicVolumeKey, 0.8f);
        }

        if (PlayerPrefs.HasKey(LegacySfxVolumeKey))
        {
            targetData.settings.sfxVolume =
                PlayerPrefs.GetFloat(LegacySfxVolumeKey, 1f);
        }

        targetData.ValidateAndClamp();
    }

    private void SyncLegacyPlayerPrefsBestScore(PracticeSaveData targetData)
    {
        if (targetData == null)
        {
            return;
        }

        if (PlayerPrefs.HasKey(LegacyBestScoreKey))
        {
            int legacyBest = PlayerPrefs.GetInt(LegacyBestScoreKey, 0);
            if (legacyBest > targetData.statistics.bestScore)
            {
                targetData.statistics.bestScore = legacyBest;
            }
        }
    }
}