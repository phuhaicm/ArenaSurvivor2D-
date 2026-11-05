#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class PracticeBenchmarkEditorTools
{
    [MenuItem("Tools/Practice/Setup Benchmark, VFX, Camera Shake & Optimize Physics2D")]
    public static void SetupBenchmarkAndOptimizePhysics2D()
    {
        OptimizePhysics2DSettings();
        OptimizeEnemyPrefabsForZeroLag();
        SetupBenchmarkInCurrentScene();
        SetupVfxAndCameraShakeInCurrentScene();
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

        // - QUAN TRỌNG NHẤT CHO BENCHMARK 1.000 QUÁI:
        // Enemy chỉ cần va chạm với Default (Đạn Player - Layer 0) và PlayerBody (Layer 3).
        // TẮT va chạm Enemy <-> Enemy để khi 1.000 con quái cùng tụ lại quanh Player không tạo ra
        // 500.000 cặp va chạm O(N^2) làm nghẽn cứng Box2D & OnTriggerStay2D!
        if (enemyLayer >= 0)
        {
            for (int i = 0; i < 32; i++)
            {
                bool shouldCollide = (i == defaultLayer || i == playerBodyLayer);
                Physics2D.IgnoreLayerCollision(enemyLayer, i, !shouldCollide);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            "<color=#4CAF50><b>[Physics2D Optimizer]</b></color> Đã đặt Gravity = (0, 0), tắt va chạm chồng chéo O(N^2) Enemy <-> Enemy và tối ưu toàn bộ Physics 2D Layer Matrix!"
        );
    }

    private static void OptimizeEnemyPrefabsForZeroLag()
    {
        string[] enemyPrefabPaths =
        {
            "Assets/Practice/Prefabs/Combat/Enemies/Practice_Enemy_Normal.prefab",
            "Assets/Practice/Prefabs/Combat/Enemies/Practice_Enemy_Fast.prefab",
            "Assets/Practice/Prefabs/Combat/Enemies/Practice_Enemy_Tank.prefab"
        };

        PracticeFloatingDamageText damageTextPrefab =
            AssetDatabase.LoadAssetAtPath<PracticeFloatingDamageText>(
                "Assets/Practice/Prefabs/Practice_FloatingDamageText.prefab"
            );

        for (int i = 0; i < enemyPrefabPaths.Length; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPrefabPaths[i]);
            if (prefab == null) continue;

            bool modified = false;

            CircleCollider2D circleCol = prefab.GetComponent<CircleCollider2D>();
            if (circleCol != null && !circleCol.isTrigger)
            {
                circleCol.isTrigger = true;
                modified = true;
            }

            PracticeEnemyDamageFeedback feedback = prefab.GetComponent<PracticeEnemyDamageFeedback>();
            if (feedback != null && damageTextPrefab != null)
            {
                SerializedObject feedbackSo = new SerializedObject(feedback);
                SerializedProperty dmgTextProp = feedbackSo.FindProperty("damageTextPrefab");
                if (dmgTextProp != null && dmgTextProp.objectReferenceValue == null)
                {
                    dmgTextProp.objectReferenceValue = damageTextPrefab;
                    feedbackSo.ApplyModifiedPropertiesWithoutUndo();
                    modified = true;
                }
            }

            if (modified)
            {
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }
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

    private static void SetupVfxAndCameraShakeInCurrentScene()
    {
        GameObject systemsRoot = GameObject.Find("PracticeGame/Systems");
        if (systemsRoot == null)
        {
            systemsRoot = GameObject.Find("Systems");
        }

        GameObject gameplayRoot = GameObject.Find("PracticeGame/Gameplay");
        if (gameplayRoot == null)
        {
            gameplayRoot = GameObject.Find("Gameplay");
        }

        if (systemsRoot == null)
        {
            return;
        }

        // 1. Đảm bảo có thùng chứa RuntimeVFX trong Gameplay
        Transform vfxContainer = FindTransform("RuntimeVFX");
        if (vfxContainer == null && gameplayRoot != null)
        {
            GameObject vfxContainerObj = new GameObject("RuntimeVFX");
            Undo.RegisterCreatedObjectUndo(vfxContainerObj, "Create RuntimeVFX");
            vfxContainerObj.transform.SetParent(gameplayRoot.transform, false);
            vfxContainer = vfxContainerObj.transform;
        }

        // 2. Đảm bảo có PracticeVFXManager trong Systems
        PracticeVFXManager vfxManager = Object.FindFirstObjectByType<PracticeVFXManager>();
        if (vfxManager == null)
        {
            GameObject vfxObj = new GameObject("PracticeVFXManager");
            Undo.RegisterCreatedObjectUndo(vfxObj, "Create PracticeVFXManager");
            vfxObj.transform.SetParent(systemsRoot.transform, false);
            vfxManager = vfxObj.AddComponent<PracticeVFXManager>();
        }

        if (vfxManager != null && vfxContainer != null)
        {
            SerializedObject vfxSo = new SerializedObject(vfxManager);
            AssignObjectReference(vfxSo, "vfxContainer", vfxContainer);
            vfxSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(vfxManager);
        }

        // 3. Đảm bảo Main Camera có gắn PracticeCameraShake2D
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            PracticeCameraShake2D shake = mainCam.GetComponent<PracticeCameraShake2D>();
            if (shake == null)
            {
                shake = Undo.AddComponent<PracticeCameraShake2D>(mainCam.gameObject);
            }

            if (shake != null)
            {
                SerializedObject shakeSo = new SerializedObject(shake);
                AssignObjectReference(shakeSo, "finalBossController", Object.FindFirstObjectByType<PracticeFinalBossController>());
                AssignObjectReference(shakeSo, "gameStateController", Object.FindFirstObjectByType<PracticeGameStateController>());
                AssignObjectReference(shakeSo, "playerLevelSystem", Object.FindFirstObjectByType<PracticePlayerLevelSystem>());
                shakeSo.ApplyModifiedProperties();
                EditorUtility.SetDirty(shake);
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log(
            "<color=#4CAF50><b>[VFX & Camera Shake Setup] HOÀN TẤT:</b></color> Đã cấu hình PracticeVFXManager, RuntimeVFX và PracticeCameraShake2D trên Main Camera!"
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
