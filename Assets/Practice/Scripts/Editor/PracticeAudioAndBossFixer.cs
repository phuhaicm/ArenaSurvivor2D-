#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PracticeAudioAndBossFixer
{
    private const string AudioLibraryPath = "Assets/Practice/Data/Audio/PracticeAudioLibrary.asset";
    private const string SfxDirectory = "Assets/Resources/Audio/SFX";
    private const string CoreLoopScenePath = "Assets/Practice/Scenes/Practice_CoreLoop.unity";
    private const string BossPrefabPath = "Assets/Practice/Prefabs/Combat/Boss/Practice_FinalBoss.prefab";
    private const string BossProjectilePrefabPath = "Assets/Practice/Prefabs/Combat/Boss/BossProjectile_Test.prefab";

    [MenuItem("Tools/Practice/Setup Audio, Screen Shake & Boss Fixes")]
    public static void SetupAudioAndBossFixes()
    {
        int actionCount = 0;

        // 1. Sinh các file âm thanh SFX còn thiếu (Shoot, PlayerHit, EnemyDeath, BossWarning)
        actionCount += GenerateMissingSfxFiles();

        // 2. Refresh AssetDatabase để Unity nhận diện và import các file WAV
        AssetDatabase.Refresh();

        // 3. Liên kết các AudioClip vào ScriptableObject PracticeAudioLibrary
        actionCount += WireAudioLibraryClips();

        // 4. Sửa và chuẩn hóa Prefab Đạn Boss (BossProjectile_Test)
        actionCount += FixBossProjectilePrefab();

        // 5. Sửa và chuẩn hóa Prefab Boss (Practice_FinalBoss)
        actionCount += FixBossPrefabExecutors();

        // 6. Gắn PracticeSceneAudioTrigger và PracticePlayerAudioFeedback vào Scene
        actionCount += SetupSceneAudioComponents();

        AssetDatabase.SaveAssets();

        Debug.Log(
            $"<color=#4CAF50><b>[Audio & Boss Fixer] THÀNH CÔNG:</b></color> " +
            $"Đã thực hiện <b>{actionCount}</b> thao tác cấu hình Audio, Screen Shake và sửa lỗi Boss!"
        );
    }

    private static int GenerateMissingSfxFiles()
    {
        if (!Directory.Exists(SfxDirectory))
        {
            Directory.CreateDirectory(SfxDirectory);
        }

        int generatedCount = 0;

        // 1. Shoot.wav (Laser Pew-pew: 950Hz -> 250Hz, 0.08s)
        string shootPath = Path.Combine(SfxDirectory, "Shoot.wav");
        if (!File.Exists(shootPath))
        {
            CreateWavFile(shootPath, 0.08f, (t, norm) =>
            {
                float freq = 950f - 700f * norm;
                float amp = 1f - norm;
                return Mathf.Sin(2f * Mathf.PI * freq * t) * amp * 0.45f;
            });
            generatedCount++;
        }

        // 2. PlayerHit.wav (Punchy Thud: 160Hz -> 45Hz với decay nhanh, 0.12s)
        string hitPath = Path.Combine(SfxDirectory, "PlayerHit.wav");
        if (!File.Exists(hitPath))
        {
            var rand = new System.Random(42);
            CreateWavFile(hitPath, 0.12f, (t, norm) =>
            {
                float freq = 160f - 115f * norm;
                float env = Mathf.Exp(-22f * norm);
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0);
                return (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f + noise * 0.3f) * env * 0.55f;
            });
            generatedCount++;
        }

        // 3. EnemyDeath.wav (Giòn tan / vỡ vụn: white noise + low rumble decay, 0.15s)
        string deathPath = Path.Combine(SfxDirectory, "EnemyDeath.wav");
        if (!File.Exists(deathPath))
        {
            var rand = new System.Random(99);
            CreateWavFile(deathPath, 0.15f, (t, norm) =>
            {
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0);
                float freq = 130f * (1f - norm);
                float env = (1f - norm) * (1f - norm);
                return (noise * 0.75f + Mathf.Sin(2f * Mathf.PI * freq * t) * 0.25f) * env * 0.45f;
            });
            generatedCount++;
        }

        // 4. BossWarning.wav (Còi báo động dồn dập 2 tông: 520Hz & 780Hz, 0.45s)
        string warningPath = Path.Combine(SfxDirectory, "BossWarning.wav");
        if (!File.Exists(warningPath))
        {
            CreateWavFile(warningPath, 0.45f, (t, norm) =>
            {
                int phase = ((int)(t * 6f)) % 2;
                float freq = phase == 0 ? 520f : 780f;
                float pulse = Mathf.Sin(2f * Mathf.PI * 12f * t);
                return Mathf.Sin(2f * Mathf.PI * freq * t) * (0.6f + 0.4f * pulse) * 0.45f;
            });
            generatedCount++;
        }

        return generatedCount;
    }

    private static void CreateWavFile(string filePath, float durationSeconds, Func<float, float, float> sampleFunc)
    {
        const int sampleRate = 44100;
        int numSamples = Mathf.CeilToInt(sampleRate * durationSeconds);
        int subChunk2Size = numSamples * 2;
        int chunkSize = 36 + subChunk2Size;

        using (var fs = new FileStream(filePath, FileMode.Create))
        using (var bw = new BinaryWriter(fs))
        {
            // RIFF header
            bw.Write(Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(chunkSize);
            bw.Write(Encoding.ASCII.GetBytes("WAVE"));

            // fmt chunk
            bw.Write(Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16); // Chunk size
            bw.Write((short)1); // PCM
            bw.Write((short)1); // Mono
            bw.Write(sampleRate);
            bw.Write(sampleRate * 2); // Byte rate
            bw.Write((short)2); // Block align
            bw.Write((short)16); // Bits per sample

            // data chunk
            bw.Write(Encoding.ASCII.GetBytes("data"));
            bw.Write(subChunk2Size);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float norm = (float)i / numSamples;
                float sample = sampleFunc(t, norm);
                sample = Mathf.Clamp(sample, -1f, 1f);
                short pcm = (short)(sample * 32767f);
                bw.Write(pcm);
            }
        }
    }

    private static int WireAudioLibraryClips()
    {
        PracticeAudioLibrary library = AssetDatabase.LoadAssetAtPath<PracticeAudioLibrary>(AudioLibraryPath);
        if (library == null)
        {
            return 0;
        }

        SerializedObject serializedLib = new SerializedObject(library);
        int updatedCount = 0;

        AudioClip shootClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Audio/SFX/Shoot.wav");
        AudioClip deathClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Audio/SFX/EnemyDeath.wav");
        AudioClip hitClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Audio/SFX/PlayerHit.wav");
        AudioClip warningClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Audio/SFX/BossWarning.wav");

        SerializedProperty shootProp = serializedLib.FindProperty("shootSfx");
        if (shootProp != null && (shootProp.objectReferenceValue == null || shootProp.objectReferenceValue != shootClip))
        {
            shootProp.objectReferenceValue = shootClip;
            updatedCount++;
        }

        SerializedProperty deathProp = serializedLib.FindProperty("enemyDeathSfx");
        if (deathProp != null && (deathProp.objectReferenceValue == null || deathProp.objectReferenceValue != deathClip))
        {
            deathProp.objectReferenceValue = deathClip;
            updatedCount++;
        }

        SerializedProperty hitProp = serializedLib.FindProperty("playerHitSfx");
        if (hitProp != null && (hitProp.objectReferenceValue == null || hitProp.objectReferenceValue != hitClip))
        {
            hitProp.objectReferenceValue = hitClip;
            updatedCount++;
        }

        SerializedProperty warningProp = serializedLib.FindProperty("bossWarningSfx");
        if (warningProp != null && (warningProp.objectReferenceValue == null || warningProp.objectReferenceValue != warningClip))
        {
            warningProp.objectReferenceValue = warningClip;
            updatedCount++;
        }

        if (updatedCount > 0)
        {
            serializedLib.ApplyModifiedProperties();
            EditorUtility.SetDirty(library);
        }

        return updatedCount;
    }

    private static int FixBossProjectilePrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BossProjectilePrefabPath);
        if (prefab == null) return 0;

        SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
        if (sr == null) return 0;

        int fixes = 0;
        if (sr.sprite == null)
        {
            Sprite circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            if (circle == null)
            {
                circle = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            }
            if (circle != null)
            {
                sr.sprite = circle;
                fixes++;
            }
        }

        sr.color = new Color(1f, 0.22f, 0.15f, 1f);
        prefab.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        EditorUtility.SetDirty(prefab);
        return fixes;
    }

    private static int FixBossPrefabExecutors()
    {
        GameObject bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath);
        if (bossPrefab == null) return 0;

        PracticeBossAttackController attackController = bossPrefab.GetComponent<PracticeBossAttackController>();
        if (attackController == null) return 0;

        PracticeBossRadialAttackExecutor radial = bossPrefab.GetComponentInChildren<PracticeBossRadialAttackExecutor>(true);
        PracticeBossAimedSpreadAttackExecutor spread = bossPrefab.GetComponentInChildren<PracticeBossAimedSpreadAttackExecutor>(true);

        SerializedObject so = new SerializedObject(attackController);
        SerializedProperty executorsProp = so.FindProperty("attackExecutors");

        if (executorsProp != null)
        {
            executorsProp.ClearArray();
            if (radial != null)
            {
                executorsProp.InsertArrayElementAtIndex(0);
                executorsProp.GetArrayElementAtIndex(0).objectReferenceValue = radial;
            }
            if (spread != null)
            {
                int index = executorsProp.arraySize;
                executorsProp.InsertArrayElementAtIndex(index);
                executorsProp.GetArrayElementAtIndex(index).objectReferenceValue = spread;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(bossPrefab);
            return 1;
        }

        return 0;
    }

    private static int SetupSceneAudioComponents()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        bool isCoreLoop = currentScene.path == CoreLoopScenePath;

        if (!isCoreLoop && File.Exists(CoreLoopScenePath))
        {
            EditorSceneManager.OpenScene(CoreLoopScenePath, OpenSceneMode.Single);
        }

        int fixes = 0;

        // 1. Kiểm tra PracticeSceneAudioTrigger
        PracticeSceneAudioTrigger audioTrigger = UnityEngine.Object.FindFirstObjectByType<PracticeSceneAudioTrigger>();
        if (audioTrigger == null)
        {
            GameObject triggerGo = new GameObject("Practice_SceneAudioTrigger");
            audioTrigger = triggerGo.AddComponent<PracticeSceneAudioTrigger>();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            fixes++;
        }

        // 2. Kiểm tra PracticePlayerAudioFeedback trên Player
        PracticePlayerHealth playerHealth = UnityEngine.Object.FindFirstObjectByType<PracticePlayerHealth>();
        if (playerHealth != null)
        {
            PracticePlayerAudioFeedback audioFeedback = playerHealth.GetComponent<PracticePlayerAudioFeedback>();
            if (audioFeedback == null)
            {
                playerHealth.gameObject.AddComponent<PracticePlayerAudioFeedback>();
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                fixes++;
            }

            // Đảm bảo MaxHealth là 100 hợp lý (thay vì 10,000)
            if (playerHealth.MaxHealth > 500f)
            {
                SerializedObject playerHealthSo = new SerializedObject(playerHealth);
                SerializedProperty maxHealthProp = playerHealthSo.FindProperty("maxHealth");
                if (maxHealthProp != null)
                {
                    maxHealthProp.floatValue = 100f;
                    playerHealthSo.ApplyModifiedProperties();
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    fixes++;
                }
            }
        }

        // 3. Kiểm tra PracticeCameraShake2D trên Main Camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            PracticeCameraShake2D shaker = mainCam.GetComponent<PracticeCameraShake2D>();
            if (shaker == null)
            {
                mainCam.gameObject.AddComponent<PracticeCameraShake2D>();
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                fixes++;
            }
        }

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        return fixes;
    }
}
#endif

