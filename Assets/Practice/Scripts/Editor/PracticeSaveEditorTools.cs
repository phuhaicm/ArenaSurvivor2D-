#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class PracticeSaveEditorTools
{
    [MenuItem("Tools/Practice/Save System/1. Print Save JSON to Console")]
    public static void PrintSaveJsonToConsole()
    {
        string path = PracticeSaveManager.MainSaveFilePath;

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[Save Tools] Chưa có file save tại: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        Debug.Log(
            $"<color=#4CAF50><b>[Save Tools] Nội dung JSON ({path}):</b></color>\n{json}"
        );
    }

    [MenuItem("Tools/Practice/Save System/2. Open Save Folder in Explorer")]
    public static void OpenSaveFolder()
    {
        string folderPath = Application.persistentDataPath;

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = folderPath,
            UseShellExecute = true,
            Verb = "open"
        });
    }

    [MenuItem("Tools/Practice/Save System/3. Simulate Corrupted Save (Test Backup Recovery)")]
    public static void SimulateCorruptedMainSave()
    {
        string path = PracticeSaveManager.MainSaveFilePath;
        File.WriteAllText(path, "{ CORRUPTED_JSON_FILE_TEST !!! ");

        Debug.LogWarning(
            $"[Save Tools] Đã giả lập làm hỏng file save chính tại: {path}. " +
            "Hãy bấm Play để kiểm chứng hệ thống tự động khôi phục từ file .bak!"
        );
    }

    [MenuItem("Tools/Practice/Save System/4. Reset All Save Data")]
    public static void ResetAllSaveData()
    {
        if (File.Exists(PracticeSaveManager.MainSaveFilePath))
        {
            File.Delete(PracticeSaveManager.MainSaveFilePath);
        }

        if (File.Exists(PracticeSaveManager.BackupSaveFilePath))
        {
            File.Delete(PracticeSaveManager.BackupSaveFilePath);
        }

        if (File.Exists(PracticeSaveManager.TempSaveFilePath))
        {
            File.Delete(PracticeSaveManager.TempSaveFilePath);
        }

        PlayerPrefs.DeleteKey("PracticeBestScore");
        PlayerPrefs.Save();

        Debug.Log("<color=#FF9800><b>[Save Tools] Đã xóa sạch toàn bộ dữ liệu Save về mặc định!</b></color>");
    }
}
#endif