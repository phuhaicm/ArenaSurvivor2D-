#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExportSceneHierarchy : MonoBehaviour
{
    [MenuItem("Tools/Dump All Scenes Hierarchy to Text")]
    public static void DumpAllScenes()
    {
        var sb = new StringBuilder();
        
        // Lưu lại đường dẫn scene đang mở để quay lại sau khi quét
        string originalScenePath = SceneManager.GetActiveScene().path;

        // Quét tất cả các Scene có tick chọn trong Build Settings
        var scenes = EditorBuildSettings.scenes;

        if (scenes.Length == 0)
        {
            Debug.LogWarning("Chưa có Scene nào trong Build Settings! Đang quét Scene hiện tại.");
            DumpCurrentActiveScene(sb);
        }
        else
        {
            foreach (var sceneEntry in scenes)
            {
                if (!sceneEntry.enabled) continue;

                // Mở scene ở chế độ ngầm để đọc
                var scene = EditorSceneManager.OpenScene(sceneEntry.path, OpenSceneMode.Single);
                sb.AppendLine("==================================================");
                sb.AppendLine($"=== SCENE: {scene.name} ({sceneEntry.path}) ===");
                sb.AppendLine("==================================================");

                foreach (var root in scene.GetRootGameObjects())
                {
                    DumpGameObject(root, sb, 0);
                }
                sb.AppendLine();
            }

            // Mở lại Scene ban đầu bạn đang làm việc
            if (!string.IsNullOrEmpty(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }
        }

        string path = Path.Combine(Application.dataPath, "AllScenesDump.txt");
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
        Debug.Log("Đã xuất toàn bộ Scene thành công ra file: " + path);
    }

    private static void DumpCurrentActiveScene(StringBuilder sb)
    {
        var activeScene = SceneManager.GetActiveScene();
        sb.AppendLine($"=== SCENE: {activeScene.name} ===");
        foreach (var root in activeScene.GetRootGameObjects())
        {
            DumpGameObject(root, sb, 0);
        }
    }

    private static void DumpGameObject(GameObject obj, StringBuilder sb, int indent)
    {
        sb.Append(' ', indent * 2);
        sb.AppendLine($"[GO] {obj.name} (Active: {obj.activeSelf}, Tag: {obj.tag}, Layer: {LayerMask.LayerToName(obj.layer)})");

        var components = obj.GetComponents<Component>();
        foreach (var comp in components)
        {
            if (comp == null) continue;
            sb.Append(' ', indent * 2 + 2);
            sb.AppendLine($" - Component: {comp.GetType().Name}");
        }

        for (int i = 0; i < obj.transform.childCount; i++)
        {
            DumpGameObject(obj.transform.GetChild(i).gameObject, sb, indent + 1);
        }
    }
}
#endif
