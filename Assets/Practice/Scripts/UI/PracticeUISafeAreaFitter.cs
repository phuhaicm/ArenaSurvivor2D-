using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PracticeUISafeAreaFitter : HaiMonoBehaviour//Script này tự động tính toán Screen.safeArea và quy đổi sang hệ tọa độ chuẩn hóa (0..1) của RectTransform.anchorMin và anchorMax, giúp HUD thích ứng tức thì khi người chơi đổi độ phân giải hay co kéo cửa sổ
{
    [Header("References")]
    [SerializeField]
    private RectTransform targetRectTransform;

    private Rect lastSafeArea = Rect.zero;
    private Vector2Int lastScreenSize = Vector2Int.zero;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadRectTransform();
    }

    protected override void Awake()
    {
        base.Awake();
        LoadRectTransform();
        ApplySafeAreaIfNeeded();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ApplySafeAreaIfNeeded();
    }

    private void Update()
    {
        if (HasScreenOrSafeAreaChanged())
        {
            ApplySafeAreaIfNeeded();
        }
    }

    private void LoadRectTransform()
    {
        if (targetRectTransform != null)
        {
            return;
        }

        targetRectTransform = GetComponent<RectTransform>();
    }

    private bool HasScreenOrSafeAreaChanged()
    {
        return lastSafeArea != Screen.safeArea ||
               lastScreenSize.x != Screen.width ||
               lastScreenSize.y != Screen.height;
    }

    private void ApplySafeAreaIfNeeded()
    {
        if (targetRectTransform == null)
        {
            return;
        }

        if (Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        targetRectTransform.anchorMin = anchorMin;
        targetRectTransform.anchorMax = anchorMax;
        targetRectTransform.offsetMin = Vector2.zero;
        targetRectTransform.offsetMax = Vector2.zero;
    }
}