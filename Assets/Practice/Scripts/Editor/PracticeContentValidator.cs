#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class PracticeContentValidator
{
    private static readonly Regex SnakeCaseRegex =
        new Regex("^[a-z0-9]+(_[a-z0-9]+)*$");

    [MenuItem("Tools/Practice/Validate All Content Data")]
    public static void ValidateAllContentData()
    {
        int errorCount = 0;
        int warningCount = 0;

        errorCount += ValidateBossAttackDefinitions(ref warningCount);
        errorCount += ValidateBossPhaseDefinitions(ref warningCount);
        errorCount += ValidateGenericScriptableObjects(
            "PracticeUpgradeDefinition",
            ref warningCount
        );
        errorCount += ValidateGenericScriptableObjects(
            "PracticeEnemyTypeData",
            ref warningCount
        );

        if (errorCount == 0 && warningCount == 0)
        {
            Debug.Log(
                "<color=#4CAF50><b>[Content Validator] PASS:</b></color> " +
                "Toàn bộ ScriptableObject Data hợp lệ 100% (0 Error, 0 Warning)!"
            );
        }
        else
        {
            Debug.LogWarning(
                $"<b>[Content Validator] KẾT QUẢ:</b> Phát hiện {errorCount} lỗi (Error) " +
                $"và {warningCount} cảnh báo (Warning). Hãy kiểm tra chi tiết bên trên."
            );
        }
    }

    private static int ValidateBossAttackDefinitions(ref int warningCount)
    {
        int errors = 0;
        string[] guids = AssetDatabase.FindAssets("t:PracticeBossAttackDefinition");
        HashSet<string> seenIds = new HashSet<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PracticeBossAttackDefinition asset =
                AssetDatabase.LoadAssetAtPath<PracticeBossAttackDefinition>(path);

            if (asset == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(asset.AttackId))
            {
                Debug.LogError(
                    $"[Validator Error] {asset.name} ({path}): AttackId đang để trống!",
                    asset
                );
                errors++;
            }
            else
            {
                if (!SnakeCaseRegex.IsMatch(asset.AttackId))
                {
                    Debug.LogWarning(
                        $"[Validator Warning] {asset.name}: AttackId '{asset.AttackId}' " +
                        "nên đặt theo chuẩn lowercase snake_case (ví dụ: boss_radial_burst).",
                        asset
                    );
                    warningCount++;
                }

                if (!seenIds.Add(asset.AttackId))
                {
                    Debug.LogError(
                        $"[Validator Error] {asset.name} ({path}): Bị trùng lặp AttackId '{asset.AttackId}'!",
                        asset
                    );
                    errors++;
                }
            }

            if (asset.ProjectilePrefab == null)
            {
                Debug.LogError(
                    $"[Validator Error] {asset.name} ({path}): Thiếu ProjectilePrefab!",
                    asset
                );
                errors++;
            }

            if (asset.ProjectileCount <= 0 ||
                asset.ProjectileSpeed <= 0f ||
                asset.ProjectileDamage <= 0f ||
                asset.ProjectileLifetime <= 0f)
            {
                Debug.LogError(
                    $"[Validator Error] {asset.name} ({path}): Thông số Projectile (Count/Speed/Damage/Lifetime) phải > 0!",
                    asset
                );
                errors++;
            }
        }

        return errors;
    }

    private static int ValidateBossPhaseDefinitions(ref int warningCount)
    {
        int errors = 0;
        string[] guids = AssetDatabase.FindAssets("t:PracticeBossPhaseDefinition");
        HashSet<string> seenIds = new HashSet<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PracticeBossPhaseDefinition asset =
                AssetDatabase.LoadAssetAtPath<PracticeBossPhaseDefinition>(path);

            if (asset == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(asset.PhaseId))
            {
                Debug.LogError(
                    $"[Validator Error] {asset.name} ({path}): PhaseId đang để trống!",
                    asset
                );
                errors++;
            }
            else
            {
                if (!SnakeCaseRegex.IsMatch(asset.PhaseId))
                {
                    Debug.LogWarning(
                        $"[Validator Warning] {asset.name}: PhaseId '{asset.PhaseId}' " +
                        "nên đặt theo chuẩn lowercase snake_case (ví dụ: boss_phase_1).",
                        asset
                    );
                    warningCount++;
                }

                if (!seenIds.Add(asset.PhaseId))
                {
                    Debug.LogError(
                        $"[Validator Error] {asset.name} ({path}): Bị trùng lặp PhaseId '{asset.PhaseId}'!",
                        asset
                    );
                    errors++;
                }
            }

            if (asset.HealthThreshold <= 0f || asset.HealthThreshold > 1f)
            {
                Debug.LogError(
                    $"[Validator Error] {asset.name} ({path}): HealthThreshold ({asset.HealthThreshold}) phải nằm trong khoảng (0, 1]!",
                    asset
                );
                errors++;
            }

            if (!asset.HasValidAttacks())
            {
                Debug.LogError(
                    $"[Validator Error] {asset.name} ({path}): Danh sách Attacks đang rỗng hoặc chưa có Attack hợp lệ!",
                    asset
                );
                errors++;
            }

            if (asset.PhaseTintColor.a <= 0.01f)
            {
                Debug.LogWarning(
                    $"[Validator Warning] {asset.name}: PhaseTintColor có Alpha gần bằng 0, Boss sẽ bị tàng hình!",
                    asset
                );
                warningCount++;
            }
        }

        return errors;
    }

    private static int ValidateGenericScriptableObjects(
        string typeName,
        ref int warningCount
    )
    {
        int errors = 0;
        string[] guids = AssetDatabase.FindAssets($"t:{typeName}");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject asset =
                AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

            if (asset == null)
            {
                continue;
            }

            SerializedObject serializedObject = new SerializedObject(asset);
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.propertyType == SerializedPropertyType.ObjectReference &&
                    iterator.name.ToLowerInvariant().Contains("prefab") &&
                    iterator.objectReferenceValue == null)
                {
                    Debug.LogError(
                        $"[Validator Error] {asset.name} ({path}): Trường '{iterator.displayName}' đang bị thiếu Prefab (Null)!",
                        asset
                    );
                    errors++;
                }
            }
        }

        return errors;
    }
}
#endif