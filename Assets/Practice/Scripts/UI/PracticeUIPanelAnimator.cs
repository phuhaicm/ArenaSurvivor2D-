using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PracticeUIPanelAnimator : HaiMonoBehaviour//hiệu ứng pop in fade-in cho mọi panel  
{
    [Header("References")]
    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private RectTransform animatedTarget;

    [Header("Animation Settings (Unscaled Time)")]
    [Min(0.05f)]
    [SerializeField]
    private float duration = 0.22f;

    [Range(0f, 1f)]
    [SerializeField]
    private float startAlpha = 0f;

    [Range(0.5f, 1.2f)]
    [SerializeField]
    private float startScale = 0.88f;

    [SerializeField]
    private AnimationCurve scaleCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Coroutine animationCoroutine;
    private Vector3 initialBaseScale = Vector3.one;
    private bool hasCapturedBaseScale;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadCanvasGroup();
        LoadAnimatedTarget();
    }

    protected override void Awake()
    {
        base.Awake();

        LoadCanvasGroup();
        LoadAnimatedTarget();
        CaptureBaseScaleIfNeeded();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadCanvasGroup();
        LoadAnimatedTarget();
        CaptureBaseScaleIfNeeded();

        PlayShowAnimation();
    }

    protected override void OnDisable()
    {
        StopCurrentAnimation();
        RestoreFinalStateImmediate();

        base.OnDisable();
    }

    private void LoadCanvasGroup()
    {
        if (canvasGroup != null)
        {
            return;
        }

        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void LoadAnimatedTarget()
    {
        if (animatedTarget != null)
        {
            return;
        }

        animatedTarget = GetComponent<RectTransform>();
    }

    private void CaptureBaseScaleIfNeeded()
    {
        if (hasCapturedBaseScale || animatedTarget == null)
        {
            return;
        }

        Vector3 currentScale = animatedTarget.localScale;

        if (currentScale.sqrMagnitude > 0.001f)
        {
            initialBaseScale = currentScale;
        }
        else
        {
            initialBaseScale = Vector3.one;
        }

        hasCapturedBaseScale = true;
    }

    public void PlayShowAnimation()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        StopCurrentAnimation();

        if (duration <= 0.01f)
        {
            RestoreFinalStateImmediate();
            return;
        }

        animationCoroutine = StartCoroutine(ShowAnimationRoutine());
    }

    private IEnumerator ShowAnimationRoutine()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = startAlpha;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (animatedTarget != null)
        {
            animatedTarget.localScale = initialBaseScale * startScale;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Bắt buộc dùng unscaledDeltaTime vì các Panel Modal mở khi Time.timeScale = 0
            elapsed += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / duration);

            // Công thức Ease-Out Cubic cho độ mờ (Alpha)
            float easedAlphaT = 1f - Mathf.Pow(1f - normalizedTime, 3f);
            float evaluatedScaleT = scaleCurve != null
                ? scaleCurve.Evaluate(normalizedTime)
                : easedAlphaT;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, easedAlphaT);
            }

            if (animatedTarget != null)
            {
                float currentScaleFactor = Mathf.LerpUnclamped(
                    startScale,
                    1f,
                    evaluatedScaleT
                );
                animatedTarget.localScale = initialBaseScale * currentScaleFactor;
            }

            yield return null;
        }

        RestoreFinalStateImmediate();
        animationCoroutine = null;
    }

    private void StopCurrentAnimation()
    {
        if (animationCoroutine == null)
        {
            return;
        }

        StopCoroutine(animationCoroutine);
        animationCoroutine = null;
    }

    private void RestoreFinalStateImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (animatedTarget != null && hasCapturedBaseScale)
        {
            animatedTarget.localScale = initialBaseScale;
        }
    }
}