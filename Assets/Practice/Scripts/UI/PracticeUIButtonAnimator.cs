using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
//Script này tự động cấu hình bảng màu 5 trạng thái (Normal, Highlighted, Pressed, Selected, Disabled) và tạo hiệu ứng phóng nhẹ khi rê chuột vào (PointerEnter), thu nhỏ khi nhấn (PointerDown)
public class PracticeUIButtonAnimator : HaiMonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("References")]
    [SerializeField]
    private Button targetButton;

    [SerializeField]
    private RectTransform targetTransform;

    [Header("Scale Feedback (Unscaled Time)")]
    [Range(1f, 1.25f)]
    [SerializeField]
    private float hoverScale = 1.05f;

    [Range(0.8f, 1f)]
    [SerializeField]
    private float pressedScale = 0.94f;

    [Min(0.02f)]
    [SerializeField]
    private float transitionDuration = 0.08f;

    [Header("Standardize Color States")]
    [SerializeField]
    private bool applyStandardColorStates = true;

    [SerializeField]
    private Color normalColor = Color.white;
    
    [Header("Text Contrast Guard")]
    [SerializeField]
    private bool ensureReadableTextColor = true;

    [SerializeField]
    private Color darkTextColorForLightButton = new Color(0.09f, 0.12f, 0.18f, 1f);

    [SerializeField]
    private Color highlightedColor = new Color(0.92f, 0.96f, 1f, 1f);

    [SerializeField]
    private Color pressedColor = new Color(0.78f, 0.84f, 0.92f, 1f);

    [SerializeField]
    private Color selectedColor = new Color(0.92f, 0.96f, 1f, 1f);

    [SerializeField]
    private Color disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.6f);

    private Vector3 baseScale = Vector3.one;
    private bool hasCapturedScale;
    private bool isPointerInside;
    private Coroutine scaleCoroutine;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadButton();
        LoadTargetTransform();
        ApplyColorBlockIfNeeded();
    }

    protected override void Awake()
    {
        base.Awake();

        LoadButton();
        LoadTargetTransform();
        CaptureBaseScaleIfNeeded();
        ApplyColorBlockIfNeeded();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        CaptureBaseScaleIfNeeded();
        ResetScaleImmediate();
    }

    protected override void OnDisable()
    {
        StopScaleCoroutine();
        ResetScaleImmediate();
        isPointerInside = false;

        base.OnDisable();
    }

    private void LoadButton()
    {
        if (targetButton != null)
        {
            return;
        }

        targetButton = GetComponent<Button>();
    }

    private void LoadTargetTransform()
    {
        if (targetTransform != null)
        {
            return;
        }

        targetTransform = GetComponent<RectTransform>();
    }

    private void CaptureBaseScaleIfNeeded()
    {
        if (hasCapturedScale || targetTransform == null)
        {
            return;
        }

        Vector3 current = targetTransform.localScale;
        baseScale = current.sqrMagnitude > 0.001f ? current : Vector3.one;
        hasCapturedScale = true;
    }

    private void ApplyColorBlockIfNeeded()
    {
        if (targetButton == null)
        {
            return;
        }

        if (applyStandardColorStates)
        {
            ColorBlock colors = targetButton.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = highlightedColor;
            colors.pressedColor = pressedColor;
            colors.selectedColor = selectedColor;
            colors.disabledColor = disabledColor;
            colors.fadeDuration = transitionDuration;
            targetButton.colors = colors;
        }

        if (ensureReadableTextColor)
        {
            EnsureButtonTextContrast();
        }
    }

    private void EnsureButtonTextContrast()
    {
        Graphic targetGraphic = targetButton.targetGraphic;

        if (targetGraphic == null)
        {
            return;
        }

        // Tính độ sáng (Luminance) của màu nền nút bấm
        Color bg = targetGraphic.color * normalColor;
        float bgLuminance = (0.299f * bg.r) + (0.587f * bg.g) + (0.114f * bg.b);

        // Nếu nền nút bấm màu sáng (Luminance > 0.7) như màu trắng
        if (bgLuminance > 0.7f)
        {
            TMPro.TMP_Text[] texts = GetComponentsInChildren<TMPro.TMP_Text>(true);

            for (int i = 0; i < texts.Length; i++)
            {
                Color textColor = texts[i].color;
                float textLuminance =
                    (0.299f * textColor.r) + (0.587f * textColor.g) + (0.114f * textColor.b);

                // Nếu chữ cũng đang màu trắng/sáng (Luminance > 0.75) gây tàng hình chữ
                if (textLuminance > 0.75f)
                {
                    texts[i].color = darkTextColorForLightButton;
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;

        if (!IsButtonInteractable())
        {
            return;
        }

        AnimateToScale(hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        AnimateToScale(1f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsButtonInteractable())
        {
            AnimateToScale(1f);
            return;
        }

        float targetFactor = isPointerInside ? hoverScale : 1f;
        AnimateToScale(targetFactor);
    }

    private bool IsButtonInteractable()
    {
        return targetButton != null && targetButton.interactable;
    }

    private void AnimateToScale(float scaleMultiplier)
    {
        if (!isActiveAndEnabled || targetTransform == null)
        {
            return;
        }

        StopScaleCoroutine();
        scaleCoroutine = StartCoroutine(
            ScaleRoutine(baseScale * scaleMultiplier)
        );
    }

    private IEnumerator ScaleRoutine(Vector3 destinationScale)
    {
        Vector3 startScale = targetTransform.localScale;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            targetTransform.localScale = Vector3.Lerp(
                startScale,
                destinationScale,
                t
            );

            yield return null;
        }

        targetTransform.localScale = destinationScale;
        scaleCoroutine = null;
    }

    private void StopScaleCoroutine()
    {
        if (scaleCoroutine == null)
        {
            return;
        }

        StopCoroutine(scaleCoroutine);
        scaleCoroutine = null;
    }

    private void ResetScaleImmediate()
    {
        if (targetTransform != null && hasCapturedScale)
        {
            targetTransform.localScale = baseScale;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsButtonInteractable())
        {
            return;
        }

        if (PracticeAudioManager.Instance != null)
        {
            PracticeAudioManager.Instance.PlayButtonClickSfx();
        }

        AnimateToScale(pressedScale);
    }
}