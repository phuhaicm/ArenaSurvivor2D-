#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PracticeUILayoutAutoFixer
{
    private const string MainMenuScenePath = "Assets/Practice/Scenes/PracticeMainMenu.unity";
    private const string CoreLoopScenePath = "Assets/Practice/Scenes/Practice_CoreLoop.unity";

    [MenuItem("Tools/Practice/Fix ALL Scenes UI & Flow (MainMenu + CoreLoop)")]
    public static void FixAllScenesUIAndFlow()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        string originalScenePath = SceneManager.GetActiveScene().path;
        int totalFixes = 0;

        if (System.IO.File.Exists(MainMenuScenePath))
        {
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            totalFixes += FixActiveSceneInternal();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        if (System.IO.File.Exists(CoreLoopScenePath))
        {
            EditorSceneManager.OpenScene(CoreLoopScenePath, OpenSceneMode.Single);
            totalFixes += FixActiveSceneInternal();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        if (!string.IsNullOrEmpty(originalScenePath) &&
            originalScenePath != SceneManager.GetActiveScene().path &&
            System.IO.File.Exists(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        AssetDatabase.SaveAssets();

        Debug.Log(
            $"<color=#4CAF50><b>[UI & Flow Auto-Fixer] HOÀN TẤT TOÀN DỰ ÁN:</b></color> " +
            $"Đã tự động sửa chữa và lưu thành công <b>{totalFixes}</b> hạng mục UI & Game Flow trên cả 2 Scene " +
            $"('{MainMenuScenePath}' và '{CoreLoopScenePath}')!"
        );
    }

    [MenuItem("Tools/Practice/Fix & Standardize Current Scene UI")]
    public static void FixCurrentSceneUI()
    {
        int fixedCount = FixActiveSceneInternal();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log(
            $"<color=#4CAF50><b>[UI Auto-Fixer] HOÀN TẤT:</b></color> " +
            $"Đã tự động kiểm tra, sửa chữa {fixedCount} điểm lệch bố cục UI & Flow và lưu Scene '{SceneManager.GetActiveScene().name}'!"
        );
    }

    [MenuItem("Tools/Practice/Configure Unity External Script Editor Args (Jump to Line)")]
    public static void ConfigureExternalScriptEditorArgs()
    {
        EditorPrefs.SetString("kScriptEditorArgs", "-g \"$(File)\":$(Line):$(Column)");
        Debug.Log(
            "<color=#4CAF50><b>[Script Editor Setup]</b></color> " +
            "Đã cấu hình External Script Editor Args: <b>-g \"$(File)\":$(Line):$(Column)</b>! " +
            "Giờ đây khi double-click vào lỗi trong Console, IDE sẽ tự động nhảy thẳng đến đúng số dòng và cột bị lỗi."
        );
    }

    private static int FixActiveSceneInternal()
    {
        int fixedCount = 0;

        fixedCount += EnsureEventSystemExists();
        fixedCount += FixGameplayAndControllerFlowInScene();

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        if (canvases == null || canvases.Length == 0)
        {
            Debug.LogWarning("[UI Auto-Fixer] Không tìm thấy Canvas nào trong Scene hiện tại.");
            return fixedCount;
        }

        foreach (Canvas canvas in canvases)
        {
            if (canvas == null) continue;

            Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "Fix UI & Flow");

            fixedCount += FixCanvasScalerAndRaycaster(canvas);
            fixedCount += FixGroupingContainers(canvas.transform);
            fixedCount += RemoveAccidentalSafeAreaOnButtonsAndModals(canvas.transform);
            fixedCount += RemoveStrayNestedAndControllerButtons(canvas.transform);
            fixedCount += FixMainMenuSceneLayout(canvas.transform);
            fixedCount += FixCoreLoopSceneLayout(canvas.transform);
            fixedCount += FixLoadingCanvasLayout(canvas.transform);
            fixedCount += DisableRaycastOnAllTextsAndStyleButtons(canvas.transform);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return fixedCount;
    }

    private static int EnsureEventSystemExists()
    {
        EventSystem existing = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            if (existing.GetComponent<BaseInputModule>() == null)
            {
                existing.gameObject.AddComponent<StandaloneInputModule>();
            }
            EditorUtility.SetDirty(existing.gameObject);
            return 1;
        }

        GameObject esObj = new GameObject("EventSystem");
        esObj.AddComponent<EventSystem>();
        esObj.AddComponent<StandaloneInputModule>();
        return 1;
    }

    private static int FixGameplayAndControllerFlowInScene()
    {
        int fixes = 0;

        // 1. Vô hiệu hóa PracticeGameOverController trùng lặp (tránh tranh chấp với PracticeGameOverUI)
        PracticeGameOverController[] legacyGameOverControllers =
            Object.FindObjectsByType<PracticeGameOverController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        PracticeGameOverUI gameOverUI =
            Object.FindFirstObjectByType<PracticeGameOverUI>(FindObjectsInactive.Include);

        if (gameOverUI != null && legacyGameOverControllers != null)
        {
            foreach (PracticeGameOverController legacy in legacyGameOverControllers)
            {
                if (legacy != null && legacy.gameObject != gameOverUI.gameObject)
                {
                    legacy.enabled = false;
                    legacy.gameObject.SetActive(false);
                    EditorUtility.SetDirty(legacy.gameObject);
                    fixes++;
                }
            }
        }

        // 2. Đồng bộ các tham chiếu về đúng Practice_Player chính thức
        PracticePlayerHealth playerHealth =
            Object.FindFirstObjectByType<PracticePlayerHealth>(FindObjectsInactive.Include);

        if (playerHealth != null)
        {
            PracticePlayerLevelSystem playerLevelSystem =
                playerHealth.GetComponentInChildren<PracticePlayerLevelSystem>(true);
            PracticeShooterUpgradeApplier playerApplier =
                playerHealth.GetComponentInChildren<PracticeShooterUpgradeApplier>(true);
            PracticeShooterWithDamage playerShooter =
                playerHealth.GetComponentInChildren<PracticeShooterWithDamage>(true);
            PracticePlayerMovement playerMovement =
                playerHealth.GetComponentInChildren<PracticePlayerMovement>(true);
            PracticeXPMagnetDetector playerMagnet =
                playerHealth.GetComponentInChildren<PracticeXPMagnetDetector>(true);

            // Vô hiệu hóa Practice_Shooter đứng yên thừa dưới nhánh Combat
            PracticeShooterWithDamage[] allShooters =
                Object.FindObjectsByType<PracticeShooterWithDamage>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (PracticeShooterWithDamage s in allShooters)
            {
                if (s != null && playerShooter != null && s != playerShooter && s.gameObject != playerHealth.gameObject)
                {
                    s.gameObject.SetActive(false);
                    EditorUtility.SetDirty(s.gameObject);
                    fixes++;
                }
            }

            // Vô hiệu hóa PracticePlayerlLevelSystem thừa dưới nhánh Systems
            PracticePlayerLevelSystem[] allLevelSystems =
                Object.FindObjectsByType<PracticePlayerLevelSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (PracticePlayerLevelSystem ls in allLevelSystems)
            {
                if (ls != null && playerLevelSystem != null && ls != playerLevelSystem && ls.gameObject != playerHealth.gameObject)
                {
                    ls.gameObject.SetActive(false);
                    EditorUtility.SetDirty(ls.gameObject);
                    fixes++;
                }
            }

            // Đảm bảo PracticeShooterUpgradeApplier trên Player có đủ 4 receiver
            if (playerApplier != null)
            {
                SerializedObject soApplier = new SerializedObject(playerApplier);
                SetObjectReferenceIfFound(soApplier, "shooter", playerShooter);
                SetObjectReferenceIfFound(soApplier, "playerHealth", playerHealth);
                SetObjectReferenceIfFound(soApplier, "playerMovement", playerMovement);
                SetObjectReferenceIfFound(soApplier, "magnetDetector", playerMagnet);
                soApplier.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(playerApplier);
                fixes++;
            }

            // Đảm bảo PracticeUpgradeChoiceUI trỏ đúng vào LevelSystem và UpgradeApplier của Player
            PracticeUpgradeChoiceUI upgradeChoiceUI =
                Object.FindFirstObjectByType<PracticeUpgradeChoiceUI>(FindObjectsInactive.Include);
            if (upgradeChoiceUI != null)
            {
                SerializedObject soUpgradeUI = new SerializedObject(upgradeChoiceUI);
                SetObjectReferenceIfFound(soUpgradeUI, "levelSystem", playerLevelSystem);
                SetObjectReferenceIfFound(soUpgradeUI, "upgradeApplier", playerApplier);
                soUpgradeUI.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(upgradeChoiceUI);
                fixes++;
            }

            // Đảm bảo PracticeRunScore trỏ đúng vào LevelSystem của Player
            PracticeRunScore runScore =
                Object.FindFirstObjectByType<PracticeRunScore>(FindObjectsInactive.Include);
            if (runScore != null)
            {
                SerializedObject soRunScore = new SerializedObject(runScore);
                SetObjectReferenceIfFound(soRunScore, "playerHealth", playerHealth);
                SetObjectReferenceIfFound(soRunScore, "playerLevelSystem", playerLevelSystem);
                soRunScore.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(runScore);
                fixes++;
            }
        }

        // 3. Chuẩn hóa thời lượng vòng chơi trên PracticeRunDirector (nếu đang để <= 5s test nhanh thì nâng lên 30s)
        PracticeRunDirector runDirector =
            Object.FindFirstObjectByType<PracticeRunDirector>(FindObjectsInactive.Include);
        if (runDirector != null)
        {
            SerializedObject soDirector = new SerializedObject(runDirector);
            SerializedProperty propDuration = soDirector.FindProperty("runDuration");
            // Chỉ cứu trường hợp quên chỉnh giá trị test (<= 5s). Giá trị chính thức là 180s (3 phút).
            if (propDuration != null && propDuration.floatValue <= 5f)
            {
                propDuration.floatValue = 180f;
            }
            PracticePlayerKillCounter killCounter =
                Object.FindFirstObjectByType<PracticePlayerKillCounter>(FindObjectsInactive.Include);
            SetObjectReferenceIfFound(soDirector, "killCounter", killCounter);
            soDirector.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(runDirector);
            fixes++;
        }

        return fixes;
    }

    private static int FixCanvasScalerAndRaycaster(Canvas canvas)
    {
        if (canvas == null || !canvas.isRootCanvas)
        {
            return 0;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        EditorUtility.SetDirty(scaler);

        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
        raycaster.enabled = true;
        EditorUtility.SetDirty(raycaster);

        return 1;
    }

    private static int FixGroupingContainers(Transform canvasTransform)
    {
        int fixes = 0;
        string[] containerNames = { "Controller", "HUD", "Panel" };

        foreach (string containerName in containerNames)
        {
            Transform container = canvasTransform.Find(containerName);
            if (container == null) continue;

            container.gameObject.SetActive(true);

            Image strayImage = container.GetComponent<Image>();
            if (strayImage != null)
            {
                Object.DestroyImmediate(strayImage, true);
                fixes++;
            }

            PracticeUIPanelAnimator strayAnimator = container.GetComponent<PracticeUIPanelAnimator>();
            if (strayAnimator != null)
            {
                Object.DestroyImmediate(strayAnimator, true);
                fixes++;
            }

            CanvasGroup strayCg = container.GetComponent<CanvasGroup>();
            if (strayCg != null)
            {
                Object.DestroyImmediate(strayCg, true);
                fixes++;
            }

            RectTransform rt = container.GetComponent<RectTransform>();
            if (rt != null)
            {
                SetFullScreenStretch(rt);
                EditorUtility.SetDirty(rt);
                fixes++;
            }

            // Gỡ PracticeUIPanelAnimator & CanvasGroup gắn nhầm trên các Controller con
            if (containerName == "Controller")
            {
                for (int i = 0; i < container.childCount; i++)
                {
                    Transform childCtrl = container.GetChild(i);
                    PracticeUIPanelAnimator ctrlAnim = childCtrl.GetComponent<PracticeUIPanelAnimator>();
                    if (ctrlAnim != null)
                    {
                        Object.DestroyImmediate(ctrlAnim, true);
                        fixes++;
                    }

                    CanvasGroup ctrlCg = childCtrl.GetComponent<CanvasGroup>();
                    if (ctrlCg != null)
                    {
                        Object.DestroyImmediate(ctrlCg, true);
                        fixes++;
                    }
                }
            }
        }

        return fixes;
    }

    private static int RemoveAccidentalSafeAreaOnButtonsAndModals(Transform canvasTransform)
    {
        int fixes = 0;
        PracticeUISafeAreaFitter[] fitters =
            canvasTransform.GetComponentsInChildren<PracticeUISafeAreaFitter>(true);

        foreach (PracticeUISafeAreaFitter fitter in fitters)
        {
            if (fitter == null) continue;

            if (fitter.gameObject.name != "HUD")
            {
                Object.DestroyImmediate(fitter, true);
                fixes++;
            }
        }

        return fixes;
    }

    private static int RemoveStrayNestedAndControllerButtons(Transform canvasTransform)
    {
        int fixes = 0;
        Button[] allButtons = canvasTransform.GetComponentsInChildren<Button>(true);

        foreach (Button btn in allButtons)
        {
            if (btn == null) continue;

            bool isControllerObject =
                btn.gameObject.name.Contains("Controller") ||
                btn.GetComponent<PracticeVictoryUI>() != null ||
                btn.GetComponent<PracticePauseMenuUI>() != null ||
                btn.GetComponent<PracticeGameOverUI>() != null ||
                btn.GetComponent<PracticeUpgradeChoiceUI>() != null;

            bool isChildOfAnotherButton =
                btn.transform.parent != null &&
                btn.transform.parent.GetComponentInParent<Button>(true) != null;

            if (isControllerObject || isChildOfAnotherButton)
            {
                PracticeUIButtonAnimator btnAnim = btn.GetComponent<PracticeUIButtonAnimator>();
                if (btnAnim != null)
                {
                    Object.DestroyImmediate(btnAnim, true);
                }

                Object.DestroyImmediate(btn, true);
                fixes++;
            }
        }

        return fixes;
    }

    private static int FixMainMenuSceneLayout(Transform canvasTransform)
    {
        int fixes = 0;

        // Gỡ PracticeUIPanelAnimator gắn nhầm trên PracticeSettingsUI controller
        PracticeSettingsUI settingsUI = canvasTransform.GetComponentInChildren<PracticeSettingsUI>(true);
        if (settingsUI != null && settingsUI.gameObject.name != "SettingsPanel")
        {
            PracticeUIPanelAnimator strayAnim = settingsUI.GetComponent<PracticeUIPanelAnimator>();
            if (strayAnim != null)
            {
                Object.DestroyImmediate(strayAnim, true);
                fixes++;
            }
            CanvasGroup strayCg = settingsUI.GetComponent<CanvasGroup>();
            if (strayCg != null)
            {
                Object.DestroyImmediate(strayCg, true);
                fixes++;
            }
        }

        // 1. MainMenuPanel
        Transform mainMenuPanel = FindDeepChild(canvasTransform, "MainMenuPanel");
        if (mainMenuPanel != null)
        {
            mainMenuPanel.gameObject.SetActive(true);
            RectTransform panelRt = mainMenuPanel.GetComponent<RectTransform>();
            SetFullScreenStretch(panelRt);

            Image bgImage = EnsureImage(mainMenuPanel.gameObject, new Color(0.07f, 0.10f, 0.17f, 1f));
            bgImage.raycastTarget = true;

            Transform titleTr = FindDeepChild(mainMenuPanel, "GameTitleText");
            if (titleTr != null)
            {
                ConfigureTextElement(
                    titleTr,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 210f),
                    new Vector2(1000f, 120f),
                    "ARENA SURVIVOR 2D",
                    64f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Center,
                    new Color(0.98f, 0.80f, 0.08f, 1f)
                );
            }

            Transform playBtnTr = FindDeepChild(mainMenuPanel, "PlayButton");
            if (playBtnTr != null)
            {
                ConfigureButtonElement(
                    playBtnTr,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 35f),
                    new Vector2(340f, 68f),
                    "PLAY GAME",
                    28f
                );
            }

            Transform settingsBtnTr = FindDeepChild(mainMenuPanel, "SettingsButton");
            if (settingsBtnTr != null)
            {
                ConfigureButtonElement(
                    settingsBtnTr,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -55f),
                    new Vector2(340f, 68f),
                    "SETTINGS",
                    28f
                );
            }

            Transform quitBtnTr = FindDeepChild(mainMenuPanel, "QuitButton");
            if (quitBtnTr != null)
            {
                ConfigureButtonElement(
                    quitBtnTr,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -145f),
                    new Vector2(340f, 68f),
                    "QUIT",
                    28f
                );
            }

            fixes++;
        }

        // 2. SettingsPanel
        Transform settingsPanel = FindDeepChild(canvasTransform, "SettingsPanel");
        if (settingsPanel != null)
        {
            RectTransform rt = settingsPanel.GetComponent<RectTransform>();
            SetCenteredRect(rt, new Vector2(0f, 0f), new Vector2(680f, 540f));

            Image bg = EnsureImage(settingsPanel.gameObject, new Color(0.09f, 0.13f, 0.22f, 0.98f));
            bg.raycastTarget = true;

            EnsurePanelAnimatorOnModal(settingsPanel.gameObject);

            Transform titleTr = FindDeepChild(settingsPanel, "SettingsTitleText");
            if (titleTr != null)
            {
                ConfigureTextElement(
                    titleTr,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 195f),
                    new Vector2(580f, 64f),
                    "AUDIO SETTINGS",
                    40f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Center,
                    new Color(0.98f, 0.80f, 0.08f, 1f)
                );
            }

            ConfigureSliderWithLabel(settingsPanel, "MasterVolumeSlider", "MasterVolumeLabel", "Master Volume", 85f);
            ConfigureSliderWithLabel(settingsPanel, "MusicVolumeSlider", "MusicVolumeLabel", "Music Volume", 10f);
            ConfigureSliderWithLabel(settingsPanel, "SfxVolumeSlider", "SfxVolumeLabel", "SFX Volume", -65f);

            Transform backBtnTr = FindDeepChild(settingsPanel, "BackButton");
            if (backBtnTr != null)
            {
                ConfigureButtonElement(
                    backBtnTr,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -185f),
                    new Vector2(260f, 60f),
                    "BACK",
                    26f
                );
            }

            // Đảm bảo SettingsPanel ẩn lúc khởi đầu để không đè lên MainMenuPanel
            settingsPanel.gameObject.SetActive(false);
            EditorUtility.SetDirty(settingsPanel.gameObject);
            fixes++;
        }

        return fixes;
    }

    private static void ConfigureSliderWithLabel(
        Transform settingsPanel,
        string sliderName,
        string labelObjName,
        string labelContent,
        float rowY
    )
    {
        Transform sliderTr = FindDeepChild(settingsPanel, sliderName);
        if (sliderTr == null) return;

        RectTransform sliderRt = sliderTr.GetComponent<RectTransform>();
        if (sliderRt != null)
        {
            SetCenteredRect(sliderRt, new Vector2(105f, rowY), new Vector2(330f, 32f));
        }

        Transform labelTr = settingsPanel.Find(labelObjName);
        if (labelTr == null)
        {
            GameObject labelGo = new GameObject(labelObjName, typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.layer = settingsPanel.gameObject.layer;
            labelTr = labelGo.transform;
            labelTr.SetParent(settingsPanel, false);
        }

        ConfigureTextElement(
            labelTr,
            new Vector2(0.5f, 0.5f),
            new Vector2(-165f, rowY),
            new Vector2(210f, 40f),
            labelContent,
            24f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            new Color(0.85f, 0.90f, 0.96f, 1f)
        );
    }

    private static int FixCoreLoopSceneLayout(Transform canvasTransform)
    {
        int fixes = 0;

        // 1. HUD Layout
        Transform hud = canvasTransform.Find("HUD");
        if (hud != null)
        {
            Transform survivalTimerTr = hud.Find("SurvivalTimerText");
            if (survivalTimerTr != null)
            {
                ConfigureAnchoredText(
                    survivalTimerTr,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -18f),
                    new Vector2(320f, 52f),
                    38f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Center,
                    Color.white
                );
            }

            Transform healthTextTr = hud.Find("HealthText");
            if (healthTextTr != null)
            {
                ConfigureAnchoredText(
                    healthTextTr,
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(28f, -20f),
                    new Vector2(320f, 36f),
                    24f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Left,
                    Color.white
                );
            }

            Transform healthSliderTr = hud.Find("HealthSlider");
            if (healthSliderTr != null)
            {
                RectTransform rt = healthSliderTr.GetComponent<RectTransform>();
                SetRect(rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -58f), new Vector2(280f, 24f));
            }

            Transform levelTextTr = hud.Find("LevelText");
            if (levelTextTr != null)
            {
                ConfigureAnchoredText(
                    levelTextTr,
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(28f, -92f),
                    new Vector2(320f, 36f),
                    24f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Left,
                    new Color(0.98f, 0.85f, 0.25f, 1f)
                );
            }

            Transform xpSliderTr = hud.Find("XPSlider");
            if (xpSliderTr != null)
            {
                RectTransform rt = xpSliderTr.GetComponent<RectTransform>();
                SetRect(rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -130f), new Vector2(280f, 20f));
            }

            Transform killCountTr = hud.Find("KillCountText");
            if (killCountTr != null)
            {
                ConfigureAnchoredText(
                    killCountTr,
                    new Vector2(1f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(-28f, -20f),
                    new Vector2(360f, 36f),
                    24f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Right,
                    Color.white
                );
            }

            Transform finalScoreTr = hud.Find("FinalScoreText");
            if (finalScoreTr != null)
            {
                ConfigureAnchoredText(
                    finalScoreTr,
                    new Vector2(1f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(-28f, -58f),
                    new Vector2(360f, 36f),
                    24f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Right,
                    Color.white
                );
            }

            Transform bestScoreTr = hud.Find("BestScoreText");
            if (bestScoreTr != null)
            {
                ConfigureAnchoredText(
                    bestScoreTr,
                    new Vector2(1f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(-28f, -96f),
                    new Vector2(360f, 36f),
                    24f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Right,
                    new Color(0.98f, 0.80f, 0.08f, 1f)
                );
            }

            fixes++;
        }

        Transform panelGroup = canvasTransform.Find("Panel");
        if (panelGroup == null)
        {
            return fixes;
        }

        // 2. BossHealthPanel & BossWarningPanel
        Transform bossHealthPanel = panelGroup.Find("BossHealthPanel");
        if (bossHealthPanel != null)
        {
            RectTransform rt = bossHealthPanel.GetComponent<RectTransform>();
            SetRect(
                rt,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -78f),
                new Vector2(680f, 76f)
            );

            Image bg = EnsureImage(bossHealthPanel.gameObject, new Color(0.08f, 0.11f, 0.18f, 0.94f));
            bg.raycastTarget = false;

            Transform bossNameTr = bossHealthPanel.Find("BossNameText");
            if (bossNameTr != null)
            {
                ConfigureAnchoredText(
                    bossNameTr,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -6f),
                    new Vector2(640f, 30f),
                    22f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Center,
                    new Color(1f, 0.45f, 0.45f, 1f)
                );
            }

            Transform bossSliderTr = bossHealthPanel.Find("BossHealthSlider");
            if (bossSliderTr != null)
            {
                RectTransform sRt = bossSliderTr.GetComponent<RectTransform>();
                SetRect(
                    sRt,
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(0f, 12f),
                    new Vector2(620f, 26f)
                );
            }

            Transform bossHealthTextTr = bossHealthPanel.Find("BossHealthText");
            if (bossHealthTextTr != null)
            {
                ConfigureAnchoredText(
                    bossHealthTextTr,
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(0f, 12f),
                    new Vector2(300f, 26f),
                    18f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Center,
                    Color.white
                );
            }

            bossHealthPanel.gameObject.SetActive(false);
            fixes++;
        }

        Transform bossWarningPanel = panelGroup.Find("BossWarningPanel");
        if (bossWarningPanel != null)
        {
            RectTransform rt = bossWarningPanel.GetComponent<RectTransform>();
            SetRect(
                rt,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -166f),
                new Vector2(740f, 72f)
            );

            CanvasGroup cg = bossWarningPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.blocksRaycasts = false;
                cg.interactable = false;
                cg.alpha = 0f;
            }

            Image bg = bossWarningPanel.GetComponent<Image>();
            if (bg != null)
            {
                bg.raycastTarget = false;
            }

            bossWarningPanel.gameObject.SetActive(false);
            fixes++;
        }

        // 3. PracticeUpgradePanel (Gỡ VerticalLayoutGroup gây bẹp Reroll/Skip & căn giữa chuẩn)
        Transform upgradePanel = panelGroup.Find("PracticeUpgradePanel");
        if (upgradePanel != null)
        {
            VerticalLayoutGroup vlg = upgradePanel.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                Object.DestroyImmediate(vlg, true);
            }

            ContentSizeFitter csf = upgradePanel.GetComponent<ContentSizeFitter>();
            if (csf != null)
            {
                Object.DestroyImmediate(csf, true);
            }

            RectTransform rt = upgradePanel.GetComponent<RectTransform>();
            SetCenteredRect(rt, new Vector2(0f, -20f), new Vector2(760f, 560f));

            Image bg = EnsureImage(upgradePanel.gameObject, new Color(0.08f, 0.12f, 0.21f, 0.97f));
            bg.raycastTarget = true;

            EnsurePanelAnimatorOnModal(upgradePanel.gameObject);

            Transform titleTr = upgradePanel.Find("UpgradeTitleText");
            if (titleTr != null)
            {
                RemoveLayoutElementIfPresent(titleTr.gameObject);
                ConfigureAnchoredText(
                    titleTr,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -24f),
                    new Vector2(700f, 52f),
                    30f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Center,
                    new Color(0.98f, 0.80f, 0.08f, 1f)
                );
            }

            string[] choiceNames = { "ChoiceButton1", "ChoiceButton2", "ChoiceButton3" };
            float[] choiceYs = { -92f, -200f, -308f };

            for (int i = 0; i < choiceNames.Length; i++)
            {
                Transform btnTr = upgradePanel.Find(choiceNames[i]);
                if (btnTr != null)
                {
                    RemoveLayoutElementIfPresent(btnTr.gameObject);
                    ConfigureAnchoredButton(
                        btnTr,
                        new Vector2(0.5f, 1f),
                        new Vector2(0.5f, 1f),
                        new Vector2(0.5f, 1f),
                        new Vector2(0f, choiceYs[i]),
                        new Vector2(660f, 92f),
                        null,
                        22f
                    );
                }
            }

            Transform rerollTr = upgradePanel.Find("RerollButton");
            if (rerollTr != null)
            {
                RemoveLayoutElementIfPresent(rerollTr.gameObject);
                ConfigureAnchoredButton(
                    rerollTr,
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(-165f, 30f),
                    new Vector2(290f, 58f),
                    "REROLL (1)",
                    24f
                );
            }

            Transform skipTr = upgradePanel.Find("SkipButton");
            if (skipTr != null)
            {
                RemoveLayoutElementIfPresent(skipTr.gameObject);
                ConfigureAnchoredButton(
                    skipTr,
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(165f, 30f),
                    new Vector2(290f, 58f),
                    "SKIP",
                    24f
                );
            }

            upgradePanel.gameObject.SetActive(false);
            fixes++;
        }

        // 4. VictoryPanel & VictoryCard
        Transform victoryPanel = panelGroup.Find("VictoryPanel");
        if (victoryPanel != null)
        {
            RectTransform vpRt = victoryPanel.GetComponent<RectTransform>();
            SetFullScreenStretch(vpRt);

            EnsurePanelAnimatorOnModal(victoryPanel.gameObject);

            Transform victoryCard = victoryPanel.Find("VictoryCard");
            if (victoryCard != null)
            {
                RectTransform cardRt = victoryCard.GetComponent<RectTransform>();
                SetCenteredRect(cardRt, Vector2.zero, new Vector2(680f, 640f));

                Image cardBg = EnsureImage(victoryCard.gameObject, new Color(0.09f, 0.13f, 0.22f, 0.98f));
                cardBg.raycastTarget = true;

                Transform accentTop = victoryCard.Find("AccentTop");
                if (accentTop != null)
                {
                    RectTransform aRt = accentTop.GetComponent<RectTransform>();
                    SetRect(aRt, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 8f));
                }

                Transform titleTr = victoryCard.Find("VictoryTitleText");
                if (titleTr != null)
                {
                    ConfigureAnchoredText(
                        titleTr,
                        new Vector2(0.5f, 1f),
                        new Vector2(0.5f, 1f),
                        new Vector2(0.5f, 1f),
                        new Vector2(0f, -28f),
                        new Vector2(600f, 68f),
                        54f,
                        FontStyles.Bold,
                        TextAlignmentOptions.Center,
                        new Color(0.98f, 0.80f, 0.08f, 1f)
                    );
                }

                Transform subTr = victoryCard.Find("VictorySubtitleText");
                if (subTr != null)
                {
                    ConfigureAnchoredText(
                        subTr,
                        new Vector2(0.5f, 1f),
                        new Vector2(0.5f, 1f),
                        new Vector2(0.5f, 1f),
                        new Vector2(0f, -98f),
                        new Vector2(540f, 36f),
                        22f,
                        FontStyles.Bold,
                        TextAlignmentOptions.Center,
                        new Color(0.75f, 0.82f, 0.92f, 1f)
                    );
                }

                Transform divider = victoryCard.Find("Divider");
                if (divider != null)
                {
                    RectTransform dRt = divider.GetComponent<RectTransform>();
                    SetRect(dRt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(560f, 2f));
                }

                // Sắp xếp 4 dòng thống kê (Survival Time, Kill Count, Final Score, Best Score) theo đúng thứ tự trong VictoryCard
                ArrangeVictoryCardStatRows(victoryCard);

                Transform dividerBottom = victoryCard.Find("DividerBottom");
                if (dividerBottom != null)
                {
                    RectTransform dbRt = dividerBottom.GetComponent<RectTransform>();
                    SetRect(dbRt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -455f), new Vector2(560f, 2f));
                }

                Transform restartBtnTr = victoryCard.Find("RestartButton");
                if (restartBtnTr != null)
                {
                    ConfigureAnchoredButton(
                        restartBtnTr,
                        new Vector2(0.5f, 0f),
                        new Vector2(0.5f, 0f),
                        new Vector2(0.5f, 0f),
                        new Vector2(-150f, 42f),
                        new Vector2(260f, 64f),
                        "RESTART",
                        26f
                    );
                }

                Transform mainMenuBtnTr = victoryCard.Find("MainMenuButton");
                if (mainMenuBtnTr != null)
                {
                    ConfigureAnchoredButton(
                        mainMenuBtnTr,
                        new Vector2(0.5f, 0f),
                        new Vector2(0.5f, 0f),
                        new Vector2(0.5f, 0f),
                        new Vector2(150f, 42f),
                        new Vector2(260f, 64f),
                        "MAIN MENU",
                        26f
                    );
                }
            }

            victoryPanel.gameObject.SetActive(false);
            fixes++;
        }

        // 5. PracticeGameOverPanel
        Transform gameOverPanel = panelGroup.Find("PracticeGameOverPanel");
        if (gameOverPanel != null)
        {
            RectTransform rt = gameOverPanel.GetComponent<RectTransform>();
            SetCenteredRect(rt, Vector2.zero, new Vector2(640f, 560f));

            Image bg = EnsureImage(gameOverPanel.gameObject, new Color(0.11f, 0.09f, 0.16f, 0.98f));
            bg.raycastTarget = true;

            EnsurePanelAnimatorOnModal(gameOverPanel.gameObject);

            Transform goText = gameOverPanel.Find("GameOverText") ?? gameOverPanel.Find("GameOverTitleText");
            if (goText != null)
            {
                ConfigureTextElement(
                    goText,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 195f),
                    new Vector2(560f, 68f),
                    "GAME OVER",
                    52f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Center,
                    new Color(0.97f, 0.44f, 0.44f, 1f)
                );
            }

            Transform finalTime = gameOverPanel.Find("FinalTimeText");
            if (finalTime != null)
            {
                ConfigureTextElement(
                    finalTime,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 105f),
                    new Vector2(500f, 42f),
                    "Time: 00:00",
                    28f,
                    FontStyles.Normal,
                    TextAlignmentOptions.Center,
                    Color.white
                );
            }

            Transform finalKills = gameOverPanel.Find("FinalKillCountText");
            if (finalKills != null)
            {
                ConfigureTextElement(
                    finalKills,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 50f),
                    new Vector2(500f, 42f),
                    "Kills: 0",
                    28f,
                    FontStyles.Normal,
                    TextAlignmentOptions.Center,
                    Color.white
                );
            }

            Transform finalScore = gameOverPanel.Find("FinalScoreText");
            if (finalScore != null)
            {
                ConfigureTextElement(
                    finalScore,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -5f),
                    new Vector2(500f, 44f),
                    "Score: 0",
                    30f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Center,
                    new Color(0.98f, 0.80f, 0.08f, 1f)
                );
            }

            Transform restartBtn = gameOverPanel.Find("RestartButton");
            if (restartBtn != null)
            {
                ConfigureButtonElement(
                    restartBtn,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(-145f, -110f),
                    new Vector2(250f, 60f),
                    "RESTART",
                    25f
                );
            }

            Transform mainMenuBtn = gameOverPanel.Find("MainMenuButton") ?? gameOverPanel.Find("GameOverMainMenuButton");
            if (mainMenuBtn != null)
            {
                ConfigureButtonElement(
                    mainMenuBtn,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(145f, -110f),
                    new Vector2(250f, 60f),
                    "MAIN MENU",
                    25f
                );
            }

            Transform quitBtn = gameOverPanel.Find("QuitButton");
            if (quitBtn != null)
            {
                ConfigureButtonElement(
                    quitBtn,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -195f),
                    new Vector2(250f, 56f),
                    "QUIT",
                    24f
                );
            }

            gameOverPanel.gameObject.SetActive(false);
            fixes++;
        }

        // 6. PausePanel & chuyển PauseMainMenuButton từ PauseUIController vào trong PausePanel
        Transform pausePanel = panelGroup.Find("PausePanel");
        if (pausePanel != null)
        {
            Transform strayPauseMainMenuBtn = FindDeepChild(canvasTransform, "PauseMainMenuButton");
            if (strayPauseMainMenuBtn != null && strayPauseMainMenuBtn.parent != pausePanel)
            {
                strayPauseMainMenuBtn.SetParent(pausePanel, false);
            }

            RectTransform rt = pausePanel.GetComponent<RectTransform>();
            SetCenteredRect(rt, Vector2.zero, new Vector2(560f, 520f));

            Image bg = EnsureImage(pausePanel.gameObject, new Color(0.08f, 0.12f, 0.21f, 0.98f));
            bg.raycastTarget = true;

            EnsurePanelAnimatorOnModal(pausePanel.gameObject);

            Transform pauseText = pausePanel.Find("PauseText");
            if (pauseText != null)
            {
                ConfigureTextElement(
                    pauseText,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 185f),
                    new Vector2(460f, 60f),
                    "PAUSED",
                    46f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Center,
                    new Color(0.98f, 0.80f, 0.08f, 1f)
                );
            }

            Transform resumeBtn = pausePanel.Find("ResumeButton");
            if (resumeBtn != null)
            {
                ConfigureButtonElement(
                    resumeBtn,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 80f),
                    new Vector2(300f, 58f),
                    "RESUME",
                    25f
                );
            }

            Transform restartBtn = pausePanel.Find("RestartButton");
            if (restartBtn != null)
            {
                ConfigureButtonElement(
                    restartBtn,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 5f),
                    new Vector2(300f, 58f),
                    "RESTART",
                    25f
                );
            }

            Transform pauseMainMenuBtn = pausePanel.Find("PauseMainMenuButton");
            if (pauseMainMenuBtn != null)
            {
                ConfigureButtonElement(
                    pauseMainMenuBtn,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -70f),
                    new Vector2(300f, 58f),
                    "MAIN MENU",
                    25f
                );
            }

            Transform quitBtn = pausePanel.Find("QuitButton");
            if (quitBtn != null)
            {
                ConfigureButtonElement(
                    quitBtn,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -145f),
                    new Vector2(300f, 58f),
                    "QUIT",
                    25f
                );
            }

            pausePanel.gameObject.SetActive(false);
            fixes++;
        }

        return fixes;
    }

    private static void ArrangeVictoryCardStatRows(Transform victoryCard)
    {
        float[] rowYs = { -185f, -248f, -311f, -374f };
        int labelIdx = 0;
        int valueIdx = 0;

        for (int i = 0; i < victoryCard.childCount; i++)
        {
            Transform child = victoryCard.GetChild(i);
            if (child == null) continue;

            if (child.name == "LabelText" && labelIdx < rowYs.Length)
            {
                RemoveLayoutElementIfPresent(child.gameObject);
                Color labelColor = labelIdx == 3
                    ? new Color(0.98f, 0.80f, 0.08f, 1f)
                    : new Color(0.75f, 0.82f, 0.92f, 1f);

                ConfigureAnchoredText(
                    child,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(-130f, rowYs[labelIdx]),
                    new Vector2(280f, 44f),
                    28f,
                    labelIdx == 3 ? FontStyles.Bold : FontStyles.Normal,
                    TextAlignmentOptions.Left,
                    labelColor
                );
                labelIdx++;
            }
            else if (child.name == "ValueText" && valueIdx < rowYs.Length)
            {
                RemoveLayoutElementIfPresent(child.gameObject);
                Color valColor = valueIdx == 3
                    ? new Color(0.98f, 0.80f, 0.08f, 1f)
                    : Color.white;

                ConfigureAnchoredText(
                    child,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(150f, rowYs[valueIdx]),
                    new Vector2(240f, 44f),
                    30f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Right,
                    valColor
                );
                valueIdx++;
            }
        }
    }

    private static int FixLoadingCanvasLayout(Transform canvasTransform)
    {
        Transform loadingPanel = FindDeepChild(canvasTransform, "LoadingPanel");
        if (loadingPanel == null)
        {
            return 0;
        }

        RectTransform panelRt = loadingPanel.GetComponent<RectTransform>();
        SetFullScreenStretch(panelRt);

        Image bg = EnsureImage(loadingPanel.gameObject, new Color(0.05f, 0.08f, 0.14f, 0.96f));
        bg.raycastTarget = true;

        CanvasGroup cg = loadingPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        Transform titleTr = FindDeepChild(loadingPanel, "LoadingTitleText");
        if (titleTr != null)
        {
            ConfigureTextElement(
                titleTr,
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 70f),
                new Vector2(600f, 64f),
                "LOADING...",
                42f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                Color.white
            );
        }

        Transform sliderTr = FindDeepChild(loadingPanel, "ProgressSlider");
        if (sliderTr != null)
        {
            RectTransform sRt = sliderTr.GetComponent<RectTransform>();
            SetCenteredRect(sRt, new Vector2(0f, -10f), new Vector2(640f, 32f));
        }

        Transform progressTextTr = FindDeepChild(loadingPanel, "ProgressText");
        if (progressTextTr != null)
        {
            ConfigureTextElement(
                progressTextTr,
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -68f),
                new Vector2(300f, 44f),
                "0%",
                28f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Color(0.98f, 0.80f, 0.08f, 1f)
            );
        }

        loadingPanel.gameObject.SetActive(false);
        return 1;
    }

    private static int DisableRaycastOnAllTextsAndStyleButtons(Transform canvasTransform)
    {
        int fixes = 0;

        TMP_Text[] allTexts = canvasTransform.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text t in allTexts)
        {
            if (t == null) continue;
            if (t.raycastTarget)
            {
                t.raycastTarget = false;
                EditorUtility.SetDirty(t);
                fixes++;
            }
        }

        Button[] allButtons = canvasTransform.GetComponentsInChildren<Button>(true);
        foreach (Button btn in allButtons)
        {
            if (btn == null) continue;

            Graphic g = btn.targetGraphic;
            if (g == null)
            {
                g = btn.GetComponent<Image>();
                btn.targetGraphic = g;
            }

            if (g != null)
            {
                g.raycastTarget = true;
                g.color = new Color(0.95f, 0.97f, 1f, 1f);
                EditorUtility.SetDirty(g);
            }

            PracticeUIButtonAnimator btnAnim = btn.GetComponent<PracticeUIButtonAnimator>();
            if (btnAnim == null)
            {
                btnAnim = btn.gameObject.AddComponent<PracticeUIButtonAnimator>();
            }
            EditorUtility.SetDirty(btnAnim);

            TMP_Text[] btnTexts = btn.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text t in btnTexts)
            {
                t.color = new Color(0.09f, 0.12f, 0.18f, 1f);
                t.raycastTarget = false;
                EditorUtility.SetDirty(t);
            }

            fixes++;
        }

        return fixes;
    }

    private static void EnsurePanelAnimatorOnModal(GameObject modalObj)
    {
        if (modalObj == null) return;

        CanvasGroup cg = modalObj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = modalObj.AddComponent<CanvasGroup>();
        }
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        EditorUtility.SetDirty(cg);

        PracticeUIPanelAnimator anim = modalObj.GetComponent<PracticeUIPanelAnimator>();
        if (anim == null)
        {
            anim = modalObj.AddComponent<PracticeUIPanelAnimator>();
        }
        EditorUtility.SetDirty(anim);
    }

    private static void ConfigureButtonElement(
        Transform btnTr,
        Vector2 anchor,
        Vector2 anchoredPos,
        Vector2 size,
        string labelText,
        float fontSize
    )
    {
        ConfigureAnchoredButton(btnTr, anchor, anchor, new Vector2(0.5f, 0.5f), anchoredPos, size, labelText, fontSize);
    }

    private static void ConfigureAnchoredButton(
        Transform btnTr,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPos,
        Vector2 size,
        string labelText,
        float fontSize
    )
    {
        if (btnTr == null) return;

        RectTransform rt = btnTr.GetComponent<RectTransform>();
        SetRect(rt, anchorMin, anchorMax, pivot, anchoredPos, size);

        Image img = EnsureImage(btnTr.gameObject, new Color(0.95f, 0.97f, 1f, 1f));
        img.raycastTarget = true;

        Button btn = btnTr.GetComponent<Button>();
        if (btn != null)
        {
            btn.targetGraphic = img;
            btn.interactable = true;
            EditorUtility.SetDirty(btn);
        }

        TMP_Text tmp = btnTr.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            RectTransform textRt = tmp.GetComponent<RectTransform>();
            SetFullScreenStretch(textRt);
            textRt.offsetMin = new Vector2(14f, 6f);
            textRt.offsetMax = new Vector2(-14f, -6f);

            if (!string.IsNullOrEmpty(labelText))
            {
                tmp.text = labelText;
            }
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.09f, 0.12f, 0.18f, 1f);
            tmp.raycastTarget = false;
            EditorUtility.SetDirty(tmp);
        }
    }

    private static void ConfigureTextElement(
        Transform textTr,
        Vector2 anchor,
        Vector2 anchoredPos,
        Vector2 size,
        string content,
        float fontSize,
        FontStyles style,
        TextAlignmentOptions alignment,
        Color color
    )
    {
        ConfigureAnchoredText(textTr, anchor, anchor, new Vector2(0.5f, 0.5f), anchoredPos, size, fontSize, style, alignment, color, content);
    }

    private static void ConfigureAnchoredText(
        Transform textTr,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPos,
        Vector2 size,
        float fontSize,
        FontStyles style,
        TextAlignmentOptions alignment,
        Color color,
        string overrideText = null
    )
    {
        if (textTr == null) return;

        RectTransform rt = textTr.GetComponent<RectTransform>();
        SetRect(rt, anchorMin, anchorMax, pivot, anchoredPos, size);

        TMP_Text tmp = textTr.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            if (!string.IsNullOrEmpty(overrideText))
            {
                tmp.text = overrideText;
            }
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;
            EditorUtility.SetDirty(tmp);
        }
    }

    private static Image EnsureImage(GameObject go, Color color)
    {
        Image img = go.GetComponent<Image>();
        if (img == null)
        {
            img = go.AddComponent<Image>();
        }
        img.color = color;
        EditorUtility.SetDirty(img);
        return img;
    }

    private static void RemoveLayoutElementIfPresent(GameObject go)
    {
        if (go == null) return;
        LayoutElement le = go.GetComponent<LayoutElement>();
        if (le != null)
        {
            Object.DestroyImmediate(le, true);
        }
    }

    private static void SetObjectReferenceIfFound(SerializedObject so, string propName, Object target)
    {
        if (so == null || target == null) return;
        SerializedProperty prop = so.FindProperty(propName);
        if (prop != null)
        {
            prop.objectReferenceValue = target;
        }
    }

    private static Transform FindDeepChild(Transform parent, string targetName)
    {
        if (parent == null) return null;
        if (parent.name == targetName) return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeepChild(parent.GetChild(i), targetName);
            if (found != null) return found;
        }

        return null;
    }

    private static void SetCenteredRect(RectTransform rt, Vector2 anchoredPos, Vector2 size)
    {
        SetRect(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPos, size);
    }

    private static void SetRect(
        RectTransform rt,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPos,
        Vector2 sizeDelta
    )
    {
        if (rt == null) return;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        rt.localScale = Vector3.one;
        EditorUtility.SetDirty(rt);
    }

    private static void SetFullScreenStretch(RectTransform rt)
    {
        if (rt == null) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        EditorUtility.SetDirty(rt);
    }
}
#endif
