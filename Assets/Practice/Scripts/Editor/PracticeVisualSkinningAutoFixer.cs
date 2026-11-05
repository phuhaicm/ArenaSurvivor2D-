#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PracticeVisualSkinningAutoFixer
{
    private const string CoreLoopScenePath = "Assets/Practice/Scenes/Practice_CoreLoop.unity";
    private const string MainMenuScenePath = "Assets/Practice/Scenes/PracticeMainMenu.unity";

    // Art Asset Paths
    private const string PlayerShipSpritePath = "Assets/Art/SpaceShooter/Sprites/Player Ships/Short-Lazer-Ship.png";
    private const string EnemyShipSpritePath = "Assets/Art/SpaceShooter/Sprites/Enemies/Enemy_01.png";
    private const string BulletLaserSpritePath = "Assets/Art/SpaceShooter/Sprites/Weapons/Ball_02.png";
    private const string BulletMachineGunSpritePath = "Assets/Art/SpaceShooter/Sprites/Weapons/Machine Gun.png";
    private const string XpPowerUpSpritePath = "Assets/Art/SpaceShooter/Sprites/Power Ups/Power Up.png";
    private const string XpPowerUpRingSpritePath = "Assets/Art/SpaceShooter/Sprites/Power Ups/Power Up Ring.png";
    private const string NebulaTopSpritePath = "Assets/Art/SpaceShooter/Sprites/Background/Nebula_1_1_Top.png";
    private const string StarsBigSpritePath = "Assets/Art/SpaceShooter/Sprites/Background/Stars-Big_1_1.png";
    private const string PurplePlanetSpritePath = "Assets/Art/SpaceShooter/Sprites/Planets/Purple-Planet-wih-Moon.png";
    private const string BluePlanetSpritePath = "Assets/Art/SpaceShooter/Sprites/Planets/blue-planet.png";

    // Prefab Paths
    private const string EnemyNormalPrefabPath = "Assets/Practice/Prefabs/Combat/Enemies/Practice_Enemy_Normal.prefab";
    private const string EnemyFastPrefabPath = "Assets/Practice/Prefabs/Combat/Enemies/Practice_Enemy_Fast.prefab";
    private const string EnemyTankPrefabPath = "Assets/Practice/Prefabs/Combat/Enemies/Practice_Enemy_Tank.prefab";
    private const string EnemyElitePrefabPath = "Assets/Practice/Prefabs/Combat/Enemies/Practice_Enemy_Elite.prefab";
    private const string FinalBossPrefabPath = "Assets/Practice/Prefabs/Combat/Boss/Practice_FinalBoss.prefab";
    private const string PlayerBulletPrefabPath = "Assets/Practice/Prefabs/Practice_Bullet.prefab";
    private const string BossProjectilePrefabPath = "Assets/Practice/Prefabs/Combat/Boss/BossProjectile_Test.prefab";
    private const string XpSmallPrefabPath = "Assets/Practice/Prefabs/XPOrb/Practice_XPOrb_Small.prefab";
    private const string XpMediumPrefabPath = "Assets/Practice/Prefabs/XPOrb/Practice_XPOrb_Medium.prefab";
    private const string XpLargePrefabPath = "Assets/Practice/Prefabs/XPOrb/Practice_XPOrb_Large.prefab";
    private const string FloatingDamageTextPrefabPath = "Assets/Practice/Prefabs/Practice_FloatingDamageText.prefab";

    [MenuItem("Tools/Practice/Apply Complete Visual Skinning & Space Art Polish")]
    public static void ApplyCompleteVisualSkinning()
    {
        int actions = 0;

        // 1. Chuẩn hóa Texture Importer cho toàn bộ Sprite trong SpaceShooter
        actions += StandardizeTextureImporters();
        AssetDatabase.Refresh();

        // 2. Skinning toàn bộ Prefab quái vật, Boss, Đạn, Hạt kinh nghiệm và Damage Text
        actions += SkinAllPrefabs();

        // 3. Nâng cấp Scene Gameplay (Player Ship, Cosmic Background, Planets, HUD, Damage Aura)
        actions += PolishCoreLoopScene();

        // 4. Nâng cấp Scene Main Menu (Cosmic Space Background)
        actions += PolishMainMenuScene();

        AssetDatabase.SaveAssets();

        Debug.Log(
            $"<color=#00E676><b>[Visual Skinning & Art Polish] HOÀN TẤT TOÀN DIỆN:</b></color> " +
            $"Đã lột xác đồ họa thành công với <b>{actions}</b> thao tác nâng cấp Sprite 2D, VFX, HUD và không gian vũ trụ!"
        );
    }

    private static int StandardizeTextureImporters()
    {
        string[] spritePaths = new string[]
        {
            PlayerShipSpritePath, EnemyShipSpritePath, BulletLaserSpritePath,
            BulletMachineGunSpritePath, XpPowerUpSpritePath, XpPowerUpRingSpritePath,
            NebulaTopSpritePath, StarsBigSpritePath, PurplePlanetSpritePath, BluePlanetSpritePath
        };

        int count = 0;
        foreach (string path in spritePaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            bool modified = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                modified = true;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                modified = true;
            }
            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                modified = true;
            }
            if (importer.filterMode != FilterMode.Bilinear)
            {
                importer.filterMode = FilterMode.Bilinear;
                modified = true;
            }
            if (importer.maxTextureSize < 512)
            {
                importer.maxTextureSize = 512;
                modified = true;
            }

            if (modified)
            {
                importer.SaveAndReimport();
                count++;
            }
        }
        return count;
    }

    private static int SkinAllPrefabs()
    {
        int count = 0;

        Sprite enemySprite = AssetDatabase.LoadAssetAtPath<Sprite>(EnemyShipSpritePath);
        Sprite laserSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BulletLaserSpritePath);
        Sprite xpSprite = AssetDatabase.LoadAssetAtPath<Sprite>(XpPowerUpSpritePath);

        // 1. Normal Enemy (Phi thuyền trinh sát Alien)
        count += ConfigurePrefabSprite(
            EnemyNormalPrefabPath,
            enemySprite,
            "Enemy",
            0,
            Color.white,
            new Vector3(0.85f, 0.85f, 1f)
        );

        // 2. Fast Enemy (Phi thuyền siêu tốc vàng chanh)
        count += ConfigurePrefabSprite(
            EnemyFastPrefabPath,
            enemySprite,
            "Enemy",
            0,
            new Color(0.85f, 1f, 0.25f, 1f),
            new Vector3(0.65f, 0.65f, 1f)
        );

        // 3. Tank Enemy (Chiến hạm bọc thép đỏ sẫm)
        count += ConfigurePrefabSprite(
            EnemyTankPrefabPath,
            enemySprite,
            "Enemy",
            0,
            new Color(1f, 0.25f, 0.25f, 1f),
            new Vector3(1.35f, 1.35f, 1f)
        );

        // 4. Elite Enemy (Chiến hạm tinh nhuệ hoàng kim)
        count += ConfigurePrefabSprite(
            EnemyElitePrefabPath,
            enemySprite,
            "Enemy",
            0,
            new Color(1f, 0.85f, 0.15f, 1f),
            new Vector3(1.15f, 1.15f, 1f)
        );

        // 5. Final Boss (Dreadnought Mothership khổng lồ tím đậm)
        count += ConfigurePrefabSprite(
            FinalBossPrefabPath,
            enemySprite,
            "Enemy",
            0,
            new Color(0.85f, 0.15f, 0.35f, 1f),
            new Vector3(2.5f, 2.5f, 1f)
        );

        // 6. Player Bullet (Tia năng lượng Laser xanh Cyan)
        count += ConfigurePrefabSprite(
            PlayerBulletPrefabPath,
            laserSprite,
            "Projectile",
            1,
            new Color(0.25f, 0.95f, 1f, 1f),
            new Vector3(0.5f, 0.5f, 1f)
        );

        // 7. Boss Projectile (Quả cầu lửa Plasma đỏ cam)
        count += ConfigurePrefabSprite(
            BossProjectilePrefabPath,
            laserSprite,
            "BossProjectile",
            2,
            new Color(1f, 0.22f, 0.15f, 1f),
            new Vector3(0.65f, 0.65f, 1f)
        );

        // 8. XP Orb Small (Hạt ngọc năng lượng lục bảo sáng rõ, xoay đều)
        count += ConfigurePrefabSprite(
            XpSmallPrefabPath,
            xpSprite,
            "Bonus",
            0,
            new Color(0.2f, 1f, 0.45f, 1f),
            new Vector3(0.85f, 0.85f, 1f)
        );

        // 9. XP Orb Medium (Hạt ngọc năng lượng lam ngọc điện quang)
        count += ConfigurePrefabSprite(
            XpMediumPrefabPath,
            xpSprite,
            "Bonus",
            0,
            new Color(0.25f, 0.7f, 1f, 1f),
            new Vector3(1.15f, 1.15f, 1f)
        );

        // 10. XP Orb Large (Hạt ngọc năng lượng hoàng kim mặt trời)
        count += ConfigurePrefabSprite(
            XpLargePrefabPath,
            xpSprite,
            "Bonus",
            0,
            new Color(1f, 0.85f, 0.15f, 1f),
            new Vector3(1.5f, 1.5f, 1f)
        );

        // 11. Floating Damage Text Prefab (World Space Canvas & Compact Font)
        count += PolishFloatingDamageTextPrefab();

        return count;
    }

    private static int ConfigurePrefabSprite(
        string prefabPath,
        Sprite sprite,
        string sortingLayerName,
        int sortingOrder,
        Color color,
        Vector3 localScale
    )
    {
        if (sprite == null) return 0;

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return 0;

        SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
        if (sr == null) return 0;

        sr.sprite = sprite;
        sr.color = color;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = sortingOrder;

        prefab.transform.localScale = localScale;

        EditorUtility.SetDirty(prefab);
        PrefabUtility.SavePrefabAsset(prefab);
        return 1;
    }

    private static int PolishFloatingDamageTextPrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FloatingDamageTextPrefabPath);
        if (prefab == null) return 0;

        Canvas canvas = prefab.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = prefab.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingLayerName = "VFX";
        canvas.sortingOrder = 100;
        canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 |
                                          AdditionalCanvasShaderChannels.Normal |
                                          AdditionalCanvasShaderChannels.Tangent;

        RectTransform rt = prefab.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = new Vector3(0.014f, 0.014f, 1f);
            rt.sizeDelta = new Vector2(60f, 25f);
        }

        TextMeshProUGUI tmp = prefab.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.fontSize = 20f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.92f, 0.23f, 1f);
            tmp.enableWordWrapping = false;
        }

        EditorUtility.SetDirty(prefab);
        PrefabUtility.SavePrefabAsset(prefab);
        return 1;
    }

    private static int PolishCoreLoopScene()
    {
        if (!File.Exists(CoreLoopScenePath)) return 0;

        Scene originalScene = SceneManager.GetActiveScene();
        if (originalScene.path != CoreLoopScenePath)
        {
            EditorSceneManager.OpenScene(CoreLoopScenePath, OpenSceneMode.Single);
        }

        int actions = 0;

        // 1. Chỉnh camera màu nền vũ trụ sâu thẳm
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.035f, 0.045f, 0.08f, 1f);
            actions++;
        }

        // 2. Nâng cấp Player Ship
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            PracticePlayerHealth health = Object.FindFirstObjectByType<PracticePlayerHealth>();
            if (health != null) playerObj = health.gameObject;
        }

        if (playerObj != null)
        {
            SpriteRenderer playerSr = playerObj.GetComponent<SpriteRenderer>();
            if (playerSr != null)
            {
                Sprite shipSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlayerShipSpritePath);
                if (shipSprite != null)
                {
                    playerSr.sprite = shipSprite;
                    playerSr.color = Color.white;
                    playerSr.sortingLayerName = "Player";
                    playerSr.sortingOrder = 0;
                    playerObj.transform.localScale = new Vector3(1.05f, 1.05f, 1f);
                    actions++;
                }
            }
        }

        // 3. Tạo hoặc nâng cấp Môi trường Vũ trụ (Cosmic Space Background & Planets)
        GameObject envRoot = GameObject.Find("Environment_CosmicSpace");
        if (envRoot == null)
        {
            envRoot = new GameObject("Environment_CosmicSpace");
            actions++;
        }

        Sprite nebulaSprite = AssetDatabase.LoadAssetAtPath<Sprite>(NebulaTopSpritePath);
        Sprite starsSprite = AssetDatabase.LoadAssetAtPath<Sprite>(StarsBigSpritePath);
        Sprite purplePlanetSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PurplePlanetSpritePath);
        Sprite bluePlanetSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BluePlanetSpritePath);

        // Child 1: Tinh vân Nebula
        Transform nebulaTr = envRoot.transform.Find("Space_Nebula");
        GameObject nebulaGo = nebulaTr != null ? nebulaTr.gameObject : new GameObject("Space_Nebula");
        nebulaGo.transform.SetParent(envRoot.transform, false);
        nebulaGo.transform.localPosition = new Vector3(0f, 0f, 0f);
        nebulaGo.transform.localScale = new Vector3(3.2f, 3.2f, 1f);
        SpriteRenderer nebulaSr = nebulaGo.GetComponent<SpriteRenderer>();
        if (nebulaSr == null) nebulaSr = nebulaGo.AddComponent<SpriteRenderer>();
        nebulaSr.sprite = nebulaSprite;
        nebulaSr.color = new Color(1f, 1f, 1f, 0.7f);
        nebulaSr.sortingLayerName = "Background";
        nebulaSr.sortingOrder = -100;

        // Child 2: Dải sao lấp lánh Stars
        Transform starsTr = envRoot.transform.Find("Space_Stars");
        GameObject starsGo = starsTr != null ? starsTr.gameObject : new GameObject("Space_Stars");
        starsGo.transform.SetParent(envRoot.transform, false);
        starsGo.transform.localPosition = new Vector3(0f, 0f, 0f);
        starsGo.transform.localScale = new Vector3(3.2f, 3.2f, 1f);
        SpriteRenderer starsSr = starsGo.GetComponent<SpriteRenderer>();
        if (starsSr == null) starsSr = starsGo.AddComponent<SpriteRenderer>();
        starsSr.sprite = starsSprite;
        starsSr.color = new Color(1f, 1f, 1f, 0.85f);
        starsSr.sortingLayerName = "Background";
        starsSr.sortingOrder = -90;

        // Child 3: Hành tinh tím mộng mơ (Góc trên bên phải)
        Transform planet1Tr = envRoot.transform.Find("Planet_PurpleMoon");
        GameObject planet1Go = planet1Tr != null ? planet1Tr.gameObject : new GameObject("Planet_PurpleMoon");
        planet1Go.transform.SetParent(envRoot.transform, false);
        planet1Go.transform.localPosition = new Vector3(7.2f, 3.6f, 0f);
        planet1Go.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
        SpriteRenderer planet1Sr = planet1Go.GetComponent<SpriteRenderer>();
        if (planet1Sr == null) planet1Sr = planet1Go.AddComponent<SpriteRenderer>();
        planet1Sr.sprite = purplePlanetSprite;
        planet1Sr.sortingLayerName = "Planets";
        planet1Sr.sortingOrder = -50;

        // Child 4: Hành tinh xanh lam (Góc dưới bên trái)
        Transform planet2Tr = envRoot.transform.Find("Planet_CyanBlue");
        GameObject planet2Go = planet2Tr != null ? planet2Tr.gameObject : new GameObject("Planet_CyanBlue");
        planet2Go.transform.SetParent(envRoot.transform, false);
        planet2Go.transform.localPosition = new Vector3(-7.5f, -3.4f, 0f);
        planet2Go.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
        SpriteRenderer planet2Sr = planet2Go.GetComponent<SpriteRenderer>();
        if (planet2Sr == null) planet2Sr = planet2Go.AddComponent<SpriteRenderer>();
        planet2Sr.sprite = bluePlanetSprite;
        planet2Sr.sortingLayerName = "Planets";
        planet2Sr.sortingOrder = -50;

        // 4. Căn chỉnh UI HUD góc trên bên phải (Kills, Score, Best Score) tránh đè chữ
        actions += PolishHUD();

        // 5. Nâng cấp hiệu ứng vòng hào quang sát thương Plasma Damage Aura (Dual Ring & Electric Edge)
        actions += PolishDamageAuraInScene();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        return actions + 4;
    }

    private static int PolishHUD()
    {
        int count = 0;

        // 1. Kills Text (Dòng 1: y = -25)
        GameObject killTextGo = GameObject.Find("KillCountText");
        if (killTextGo != null)
        {
            RectTransform rt = killTextGo.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-28f, -25f);
                rt.sizeDelta = new Vector2(360f, 36f);
                count++;
            }
        }

        // 2. Score Text (Dòng 2: y = -55)
        GameObject runScoreTextGo = GameObject.Find("RunScoreText");
        if (runScoreTextGo != null)
        {
            RectTransform rt = runScoreTextGo.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-28f, -55f);
                rt.sizeDelta = new Vector2(360f, 36f);
                count++;
            }
            TextMeshProUGUI tmp = runScoreTextGo.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.fontSize = 24f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Right;
                tmp.enableWordWrapping = false;
            }
        }

        // 3. Best Score Text (Dòng 3: y = -85)
        GameObject bestScoreTextGo = GameObject.Find("BestScoreText");
        if (bestScoreTextGo != null)
        {
            RectTransform rt = bestScoreTextGo.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-28f, -85f);
                rt.sizeDelta = new Vector2(360f, 36f);
                count++;
            }
            TextMeshProUGUI tmp = bestScoreTextGo.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.fontSize = 24f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Right;
                tmp.enableWordWrapping = false;
            }
        }

        return count;
    }

    private static int PolishDamageAuraInScene()
    {
        PracticePlayerDamageAura aura = Object.FindFirstObjectByType<PracticePlayerDamageAura>();
        if (aura == null) return 0;

        LineRenderer lr = aura.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.sortingLayerName = "VFX";
            lr.sortingOrder = 10;
        }

        Sprite ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>(XpPowerUpRingSpritePath);
        if (ringSprite != null)
        {
            // Outer Ring
            Transform outerTr = aura.transform.Find("AuraShieldOuter");
            GameObject outerGo = outerTr != null ? outerTr.gameObject : new GameObject("AuraShieldOuter");
            outerGo.transform.SetParent(aura.transform, false);
            outerGo.transform.localPosition = Vector3.zero;
            SpriteRenderer outerSr = outerGo.GetComponent<SpriteRenderer>();
            if (outerSr == null) outerSr = outerGo.AddComponent<SpriteRenderer>();
            outerSr.sprite = ringSprite;
            outerSr.color = new Color(0.2f, 0.92f, 1f, 0.75f);
            outerSr.sortingLayerName = "VFX";
            outerSr.sortingOrder = 9;

            // Inner Ring
            Transform innerTr = aura.transform.Find("AuraShieldInner");
            GameObject innerGo = innerTr != null ? innerTr.gameObject : new GameObject("AuraShieldInner");
            innerGo.transform.SetParent(aura.transform, false);
            innerGo.transform.localPosition = Vector3.zero;
            SpriteRenderer innerSr = innerGo.GetComponent<SpriteRenderer>();
            if (innerSr == null) innerSr = innerGo.AddComponent<SpriteRenderer>();
            innerSr.sprite = ringSprite;
            innerSr.color = new Color(0.15f, 0.65f, 1f, 0.45f);
            innerSr.sortingLayerName = "VFX";
            innerSr.sortingOrder = 8;
        }

        return 1;
    }

    private static int PolishMainMenuScene()
    {
        if (!File.Exists(MainMenuScenePath)) return 0;

        EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        int actions = 0;

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.035f, 0.045f, 0.08f, 1f);
            actions++;
        }

        // Tạo background vũ trụ cho Menu
        GameObject menuEnv = GameObject.Find("Menu_CosmicBackground");
        if (menuEnv == null)
        {
            menuEnv = new GameObject("Menu_CosmicBackground");
            actions++;
        }

        Sprite nebulaSprite = AssetDatabase.LoadAssetAtPath<Sprite>(NebulaTopSpritePath);
        Sprite starsSprite = AssetDatabase.LoadAssetAtPath<Sprite>(StarsBigSpritePath);
        Sprite purplePlanetSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PurplePlanetSpritePath);

        // Nebula
        Transform nebTr = menuEnv.transform.Find("Menu_Nebula");
        GameObject nebGo = nebTr != null ? nebTr.gameObject : new GameObject("Menu_Nebula");
        nebGo.transform.SetParent(menuEnv.transform, false);
        nebGo.transform.localScale = new Vector3(3f, 3f, 1f);
        SpriteRenderer nebSr = nebGo.GetComponent<SpriteRenderer>();
        if (nebSr == null) nebSr = nebGo.AddComponent<SpriteRenderer>();
        nebSr.sprite = nebulaSprite;
        nebSr.color = new Color(1f, 1f, 1f, 0.8f);
        nebSr.sortingLayerName = "Background";
        nebSr.sortingOrder = -100;

        // Stars
        Transform starTr = menuEnv.transform.Find("Menu_Stars");
        GameObject starGo = starTr != null ? starTr.gameObject : new GameObject("Menu_Stars");
        starGo.transform.SetParent(menuEnv.transform, false);
        starGo.transform.localScale = new Vector3(3f, 3f, 1f);
        SpriteRenderer starSr = starGo.GetComponent<SpriteRenderer>();
        if (starSr == null) starSr = starGo.AddComponent<SpriteRenderer>();
        starSr.sprite = starsSprite;
        starSr.sortingLayerName = "Background";
        starSr.sortingOrder = -90;

        // Planet Moon
        Transform planetTr = menuEnv.transform.Find("Menu_Planet");
        GameObject planetGo = planetTr != null ? planetTr.gameObject : new GameObject("Menu_Planet");
        planetGo.transform.SetParent(menuEnv.transform, false);
        planetGo.transform.localPosition = new Vector3(4.5f, 1.8f, 0f);
        planetGo.transform.localScale = new Vector3(1f, 1f, 1f);
        SpriteRenderer planetSr = planetGo.GetComponent<SpriteRenderer>();
        if (planetSr == null) planetSr = planetGo.AddComponent<SpriteRenderer>();
        planetSr.sprite = purplePlanetSprite;
        planetSr.sortingLayerName = "Planets";
        planetSr.sortingOrder = -50;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        return actions + 3;
    }
}
#endif
