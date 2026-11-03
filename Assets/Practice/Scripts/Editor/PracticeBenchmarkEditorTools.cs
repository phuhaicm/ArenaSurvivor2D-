#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class PracticeBenchmarkEditorTools
{
    [MenuItem("Tools/Practice/Setup Benchmark System & Optimize Physics2D")]
    public static void SetupBenchmarkAndOptimizePhysics2D()
    {
        OptimizePhysics2DSettings();
        SetupBenchmarkInCurrentScene();
    }

    [MenuItem("Tools/Practice/Optimize Physics2D Layer Matrix Only")]
    public static void OptimizePhysics2DSettings()
    {
        // 1. Game Top-down 2D không dùng trọng lực rơi tự do theo trục Y
        Physics2D.gravity = Vector2.zero;

        int defaultLayer = LayerMask.NameToLayer("Default");
        int playerBodyLayer = LayerMask.NameToLayer("PlayerBody");
        int playerSensorLayer = LayerMask.NameToLayer("PlayerSensor");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int pickupLayer = LayerMask.NameToLayer("Pickup");
        int bossProjectileLayer = LayerMask.NameToLayer("BossProjectile");

        // 2. Tắt các cặp va chạm dư thừa để tiết kiệm CPU Physics2D khi chạy 1.000 thực thể:
        // - Pickup chỉ cần tương tác với PlayerBody (nhặt) và PlayerSensor (hút nam châm)
        if (pickupLayer >= 0)
        {
            for (int i = 0; i < 32; i++)
            {
                bool shouldCollide = (i == playerBodyLayer || i == playerSensorLayer);
                Physics2D.IgnoreLayerCollision(pickupLayer, i, !shouldCollide);
            }
        }

        // - BossProjectile chỉ cần va chạm với PlayerBody
        if (bossProjectileLayer >= 0)
        {
            for (int i = 0; i < 32; i++)
            {
                bool shouldCollide = (i == playerBodyLayer);
                Physics2D.IgnoreLayerCollision(bossProjectileLayer, i, !shouldCollide);
            }
        }

        // - PlayerSensor (vòng nam châm hút XP) chỉ cần quét trúng Pickup, không cần va chạm với Enemy hay Default
        if (playerSensorLayer >= 0)
        {
            for (int i = 0; i < 32; i++)
            {
                bool shouldCollide = (i == pickupLayer);
                Physics2D.IgnoreLayerCollision(playerSensorLayer, i, !shouldCollide);
            }
        }

        // - Enemy chỉ cần va chạm với Default (đạn Player), PlayerBody (gây sát thương tiếp xúc) và Enemy khác
        if (enemyLayer >= 0)
        {
            if (pickupLayer >= 0) Physics2D.IgnoreLayerCollision(enemyLayer, pickupLayer, true);
            if (bossProjectileLayer >= 0) Physics2D.IgnoreLayerCollision(enemyLayer, bossProjectileLayer, true);
            if (playerSensorLayer >= 0) Physics2D.IgnoreLayerCollision(enemyLayer, playerSensorLayer, true);
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            "<color=#4CAF50><b>[Physics2D Optimizer]</b></color> Đã đặt Gravity = (0, 0) và tối ưu hóa Physics 2D Layer Collision Matrix!"
        );
    }

    private static void SetupBenchmarkInCurrentScene()
    {
        GameObject systemsRoot = GameObject.Find("PracticeGame/Systems");
        if (systemsRoot == null)
        {
            systemsRoot = GameObject.Find("Systems");
        }

        if (systemsRoot == null)
        {
            Debug.LogWarning(
                "[Benchmark Setup] Hãy mở Scene 'Practice_CoreLoop' trước khi chạy công cụ này!"
            );
            return;
        }

        // 1. Tạo hoặc lấy BenchmarkOverlayText trong UI/Canvas/HUD
        TextMeshProUGUI overlayText = EnsureBenchmarkOverlayUI();

        // 2. Tạo hoặc lấy PracticeBenchmarkController trong PracticeGame/Systems
        PracticeBenchmarkController controller =
            Object.FindFirstObjectByType<PracticeBenchmarkController>();

        if (controller == null)
        {
            GameObject benchmarkObj = new GameObject("PracticeBenchmarkController");
            Undo.RegisterCreatedObjectUndo(benchmarkObj, "Create PracticeBenchmarkController");
            benchmarkObj.transform.SetParent(systemsRoot.transform, false);
            controller = benchmarkObj.AddComponent<PracticeBenchmarkController>();
        }

        // 3. Tự động gán đầy đủ tất cả Reference trên Inspector bằng SerializedObject
        SerializedObject so = new SerializedObject(controller);

        AssignObjectReference(so, "enemySpawner", Object.FindFirstObjectByType<PracticeEnemySpawner>());
        AssignObjectReference(so, "gameStateController", Object.FindFirstObjectByType<PracticeGameStateController>());
        AssignObjectReference(so, "playerHealth", Object.FindFirstObjectByType<PracticePlayerHealth>());
        AssignObjectReference(so, "enemiesContainer", FindTransform("RuntimeEnemies"));
        AssignObjectReference(so, "playerBulletsContainer", FindTransform("RuntimePlayerBullets"));
        AssignObjectReference(so, "bossProjectilesContainer", FindTransform("RuntimeBossProjectiles"));
        AssignObjectReference(so, "xpOrbsContainer", FindTransform("RuntimeXPOrbs"));
        AssignObjectReference(so, "damageTextsContainer", FindTransform("RuntimeDamageTexts"));
        AssignObjectReference(so, "benchmarkOverlayText", overlayText);

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(controller);

        // Đồng bộ luôn tham chiếu mới trên PracticeEnemySpawner, PracticeSurvivalTimer, PracticeBestScore
        WireUpdatedCoreScripts();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log(
            "<color=#4CAF50><b>[Benchmark Setup] HOÀN TẤT:</b></color> Đã tạo PracticeBenchmarkController, BenchmarkOverlayText và tự động nối dây 100% Reference trong Practice_CoreLoop!"
        );
    }

    private static TextMeshProUGUI EnsureBenchmarkOverlayUI()
    {
        GameObject existingObj = GameObject.Find("BenchmarkOverlayText");
        if (existingObj != null)
        {
            TextMeshProUGUI existingTmp = existingObj.GetComponent<TextMeshProUGUI>();
            if (existingTmp != null)
            {
                ConfigureOverlayTextComponent(existingTmp);
                return existingTmp;
            }
        }

        GameObject hudRoot = GameObject.Find("UI/Canvas/HUD");
        if (hudRoot == null)
        {
            hudRoot = GameObject.Find("HUD");
        }

        if (hudRoot == null)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                hudRoot = canvas.gameObject;
            }
        }

        if (hudRoot == null)
        {
            return null;
        }

        GameObject overlayGo = new GameObject("BenchmarkOverlayText", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(overlayGo, "Create BenchmarkOverlayText");
        overlayGo.transform.SetParent(hudRoot.transform, false);

        TextMeshProUGUI tmp = overlayGo.AddComponent<TextMeshProUGUI>();
        ConfigureOverlayTextComponent(tmp);
        return tmp;
    }

    private static void ConfigureOverlayTextComponent(TextMeshProUGUI tmp)
    {
        RectTransform rt = tmp.GetComponent<RectTransform>();
        // Neo ở góc dưới bên trái (Bottom-Left) để không bao giờ đè lên Thanh máu, XP hay Điểm số ở phía trên
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(24f, 20f);
        rt.sizeDelta = new Vector2(820f, 130f);

        tmp.fontSize = 18f;
        tmp.alignment = TextAlignmentOptions.BottomLeft;
        tmp.color = new Color(0.9f, 1f, 0.9f, 0.95f);
        tmp.enableWordWrapping = false;
        tmp.raycastTarget = false;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = Color.black;
        tmp.text = "[BENCHMARK OVERLAY READY - PRESS F1..F8]";

        EditorUtility.SetDirty(tmp);
    }

    private static void WireUpdatedCoreScripts()
    {
        PracticeEnemySpawner spawner = Object.FindFirstObjectByType<PracticeEnemySpawner>();
        if (spawner != null)
        {
            SerializedObject spawnerSo = new SerializedObject(spawner);
            AssignObjectReference(spawnerSo, "runDirector", Object.FindFirstObjectByType<PracticeRunDirector>());
            spawnerSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(spawner);
        }

        PracticeSurvivalTimer timer = Object.FindFirstObjectByType<PracticeSurvivalTimer>();
        if (timer != null)
        {
            SerializedObject timerSo = new SerializedObject(timer);
            AssignObjectReference(timerSo, "gameStateController", Object.FindFirstObjectByType<PracticeGameStateController>());
            timerSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(timer);
        }

        PracticeBestScore bestScore = Object.FindFirstObjectByType<PracticeBestScore>();
        if (bestScore != null)
        {
            SerializedObject bestSo = new SerializedObject(bestScore);
            AssignObjectReference(bestSo, "gameStateController", Object.FindFirstObjectByType<PracticeGameStateController>());
            bestSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(bestScore);
        }
    }

    private static Transform FindTransform(string objectName)
    {
        GameObject go = GameObject.Find(objectName);
        return go != null ? go.transform : null;
    }

    private static void AssignObjectReference(SerializedObject so, string propertyName, Object target)
    {
        if (target == null) return;
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            prop.objectReferenceValue = target;
        }
    }
}
#endif