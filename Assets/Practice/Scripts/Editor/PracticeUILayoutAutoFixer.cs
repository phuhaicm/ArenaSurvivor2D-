#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class PracticeUILayoutAutoFixer
{
    [MenuItem("Tools/Practice/Fix & Standardize Current Scene UI")]
    public static void FixCurrentSceneUI()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        if (canvases == null || canvases.Length == 0)
        {
            Debug.LogWarning("[UI Auto-Fixer] Không tìm thấy Canvas nào trong Scene hiện tại.");
            return;
        }

        int fixedCount = 0;

        foreach (Canvas canvas in canvases)
        {
            Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "Fix UI Layout");

            fixedCount += FixCanvasScaler(canvas);
            fixedCount += FixGroupingContainers(canvas.transform);
            fixedCount += RemoveAccidentalSafeAreaOnButtonsAndModals(canvas.transform);
            fixedCount += FixCollapsedButtonsAndPanels(canvas.transform);
            fixedCount += FixButtonTextContrast(canvas.transform);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log(
            $"<color=#4CAF50><b>[UI Auto-Fixer] HOÀN TẤT:</b></color> " +
            $"Đã tự động kiểm tra và sửa chữa {fixedCount} điểm lệch bố cục UI trong Scene! " +
            "Hãy bấm Ctrl + S để lưu Scene."
        );
    }

    private static int FixCanvasScaler(Canvas canvas)
    {
        if (canvas == null || !canvas.isRootCanvas)
        {
            return 0;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();

        if (scaler == null)
        {
            return 0;
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        EditorUtility.SetDirty(scaler);
        return 1;
    }

    private static int FixGroupingContainers(Transform canvasTransform)
    {
        int fixes = 0;
        string[] containerNames = { "Controller", "HUD", "Panel" };

        foreach (string containerName in containerNames)
        {
            Transform container = canvasTransform.Find(containerName);

            if (container == null)
            {
                continue;
            }

            // 1. Nếu thư mục gom nhóm (Controller, HUD, Panel) có gắn nhầm Image trắng -> Xóa Image đó
            Image strayImage = container.GetComponent<Image>();
            if (strayImage != null)
            {
                Object.DestroyImmediate(strayImage, true);
                fixes++;
                Debug.Log($"[UI Auto-Fixer] Đã gỡ Image trắng thừa trên nhóm '{containerName}'.");
            }

            // 2. Nếu nhóm Panel hoặc Controller bị gắn nhầm PracticeUIPanelAnimator -> Gỡ ra (chỉ gắn trên từng Panel con)
            PracticeUIPanelAnimator strayAnimator = container.GetComponent<PracticeUIPanelAnimator>();
            if (strayAnimator != null)
            {
                Object.DestroyImmediate(strayAnimator, true);
                fixes++;
            }

            // 3. Ép RectTransform của Controller, HUD, Panel về chuẩn Full-Screen Stretch (0,0 -> 1,1)
            RectTransform rt = container.GetComponent<RectTransform>();
            if (rt != null)
            {
                SetFullScreenStretch(rt);
                EditorUtility.SetDirty(rt);
                fixes++;
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
            if (fitter == null)
            {
                continue;
            }

            // Chỉ giữ SafeAreaFitter trên đối tượng tên là "HUD", gỡ khỏi mọi Button hoặc Panel khác
            if (fitter.gameObject.name != "HUD")
            {
                string objName = fitter.gameObject.name;
                Object.DestroyImmediate(fitter, true);
                fixes++;
                Debug.Log(
                    $"[UI Auto-Fixer] Đã gỡ PracticeUISafeAreaFitter gắn nhầm trên '{objName}' (nguyên nhân gây Width=0, Height=0)."
                );
            }
        }

        return fixes;
    }

    private static int FixCollapsedButtonsAndPanels(Transform canvasTransform)
    {
        int fixes = 0;

        // 1. Sửa các Button bị co rúm kích thước về 0x0 (như SkipButton trong ảnh)
        Button[] allButtons = canvasTransform.GetComponentsInChildren<Button>(true);

        foreach (Button btn in allButtons)
        {
            RectTransform rt = btn.GetComponent<RectTransform>();

            if (rt == null)
            {
                continue;
            }

            // Nếu nút bấm đang bị neo cố định (Min == Max) mà Width hoặc Height <= 5 pixels
            bool isPointAnchored = Vector2.Distance(rt.anchorMin, rt.anchorMax) < 0.01f;

            if (isPointAnchored && (rt.sizeDelta.x < 10f || rt.sizeDelta.y < 10f))
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(220f, 56f);

                // Nếu là SkipButton trong PracticeUpgradePanel thì đặt ở phía dưới bảng
                if (btn.name.Contains("Skip"))
                {
                    rt.anchoredPosition = new Vector2(140f, -220f);
                }
                else if (btn.name.Contains("Reroll"))
                {
                    rt.anchoredPosition = new Vector2(-140f, -220f);
                }

                EditorUtility.SetDirty(rt);
                fixes++;
                Debug.Log($"[UI Auto-Fixer] Đã khôi phục kích thước chuẩn (220x56) cho nút '{btn.name}'.");
            }
        }

        // 2. Kiểm tra các Modal Panel chính (VictoryPanel, PracticeUpgradePanel, PracticeGameOverPanel, PausePanel)
        Transform panelGroup = canvasTransform.Find("Panel");

        if (panelGroup != null)
        {
            for (int i = 0; i < panelGroup.childCount; i++)
            {
                Transform childPanel = panelGroup.GetChild(i);
                RectTransform rt = childPanel.GetComponent<RectTransform>();

                if (rt == null)
                {
                    continue;
                }

                if (childPanel.name.Contains("BossHealthPanel"))
                {
                    // Neo BossHealthPanel cố định ở chính giữa phía trên màn hình (Top-Center)
                    rt.anchorMin = new Vector2(0.5f, 1f);
                    rt.anchorMax = new Vector2(0.5f, 1f);
                    rt.pivot = new Vector2(0.5f, 1f);
                    rt.anchoredPosition = new Vector2(0f, -95f);

                    if (rt.sizeDelta.x < 200f || rt.sizeDelta.y < 30f)
                    {
                        rt.sizeDelta = new Vector2(600f, 70f);
                    }

                    FixBossHealthInternalTexts(childPanel);
                    EditorUtility.SetDirty(rt);
                    fixes++;
                    continue;
                }

                if (childPanel.name.Contains("BossWarningPanel"))
                {
                    // Neo BossWarningPanel ở chính giữa phía trên (dưới BossHealthPanel)
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(0f, 160f);

                    if (rt.sizeDelta.x < 200f || rt.sizeDelta.y < 30f)
                    {
                        rt.sizeDelta = new Vector2(750f, 90f);
                    }

                    EditorUtility.SetDirty(rt);
                    fixes++;
                    continue;
                }

                // Các bảng Modal (Victory, Upgrade, GameOver, Pause): nếu bị tụt về góc (0,0) hoặc kích thước quá bé
                bool isCollapsed =
                    Vector2.Distance(rt.anchorMin, rt.anchorMax) < 0.01f &&
                    (rt.sizeDelta.x < 150f || rt.sizeDelta.y < 150f);

                bool isStuckAtBottomLeft =
                    rt.anchorMin == Vector2.zero &&
                    rt.anchorMax == Vector2.zero &&
                    rt.anchoredPosition.sqrMagnitude < 5f;

                if (isCollapsed || isStuckAtBottomLeft)
                {
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = Vector2.zero;
                    rt.sizeDelta = new Vector2(620f, 520f);

                    EditorUtility.SetDirty(rt);
                    fixes++;
                    Debug.Log(
                        $"[UI Auto-Fixer] Đã đưa bảng '{childPanel.name}' từ góc dưới-trái về chính giữa màn hình (620x520)."
                    );
                }
            }
        }

        return fixes;
    }

    private static void FixBossHealthInternalTexts(Transform bossHealthPanel)
    {
        Transform nameTextTr = bossHealthPanel.Find("BossNameText");
        if (nameTextTr != null)
        {
            RectTransform rt = nameTextTr.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 18f);
                rt.sizeDelta = new Vector2(550f, 36f);
            }
        }

        Transform healthTextTr = bossHealthPanel.Find("BossHealthText");
        if (healthTextTr != null)
        {
            RectTransform rt = healthTextTr.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(300f, 30f);
            }
        }
    }

    private static int FixButtonTextContrast(Transform canvasTransform)
    {
        int fixes = 0;
        Button[] allButtons = canvasTransform.GetComponentsInChildren<Button>(true);

        foreach (Button btn in allButtons)
        {
            Graphic graphic = btn.targetGraphic;
            if (graphic == null)
            {
                continue;
            }

            Color c = graphic.color;
            float bgLum = (0.299f * c.r) + (0.587f * c.g) + (0.114f * c.b);

            if (bgLum > 0.7f)
            {
                TMP_Text[] texts = btn.GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text t in texts)
                {
                    Color tc = t.color;
                    float textLum = (0.299f * tc.r) + (0.587f * tc.g) + (0.114f * tc.b);

                    if (textLum > 0.75f)
                    {
                        t.color = new Color(0.09f, 0.12f, 0.18f, 1f);
                        EditorUtility.SetDirty(t);
                        fixes++;
                        Debug.Log(
                            $"[UI Auto-Fixer] Đã đổi màu chữ trong nút '{btn.name}' sang màu tối để hiện rõ trên nền nút trắng."
                        );
                    }
                }
            }
        }

        return fixes;
    }

    private static void SetFullScreenStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }
}
#endif