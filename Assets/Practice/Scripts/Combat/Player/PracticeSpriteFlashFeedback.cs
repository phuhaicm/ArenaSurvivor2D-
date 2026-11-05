using System.Collections;
using UnityEngine;

public class PracticeSpriteFlashFeedback
    : HaiMonoBehaviour, IPracticePoolable
{
    [Header("References")]
    [SerializeField]
    private SpriteRenderer targetSprite;

    [Header("Flash Settings")]
    [SerializeField, Min(1)]
    private int flashCount = 3;

    [SerializeField, Min(0f)]
    private float flashInterval = 0.08f;

    [SerializeField]
    private Color flashColor = Color.red;

    private Color originalColor = Color.white;
    private bool hasCachedOriginalColor;
    private Coroutine flashCoroutine;

    protected override void LoadComponents()
    {
        base.LoadComponents();

        LoadTargetSprite();
    }

    protected override void Awake()
    {
        base.Awake();

        LoadTargetSprite();
        CacheOriginalColor();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LoadTargetSprite();
        CacheOriginalColor();
        RestoreOriginalColor();
    }

    protected override void OnDisable()
    {
        StopFlash();
        RestoreOriginalColor();

        base.OnDisable();
    }

    private void LoadTargetSprite()
    {
        if (targetSprite != null)
        {
            return;
        }

        targetSprite =
            GetComponentInChildren<SpriteRenderer>();
    }

    private void CacheOriginalColor()
    {
        // Guard Clause: Chỉ lưu màu gốc đúng 1 lần duy nhất lúc khởi tạo
        // để không bao giờ lưu nhầm flashColor (màu đỏ) làm màu gốc khi tái sử dụng từ Pool
        if (hasCachedOriginalColor || targetSprite == null)
        {
            return;
        }

        originalColor = targetSprite.color;
        hasCachedOriginalColor = true;
    }

    public void PlayFlash()
    {
        // Nếu đối tượng đang bị tắt (ví dụ vừa chết và về Pool trong cùng frame), không chạy Coroutine
        if (!isActiveAndEnabled || targetSprite == null)
        {
            return;
        }

        CacheOriginalColor();
        StopFlash();
        RestoreOriginalColor();

        flashCoroutine =
            StartCoroutine(
                FlashRoutine()
            );
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < flashCount; i++)
        {
            targetSprite.color = flashColor;

            if (flashInterval > 0f)
            {
                yield return WaitForSecondsRoutine(flashInterval);
            }

            targetSprite.color = originalColor;

            if (flashInterval > 0f)
            {
                yield return WaitForSecondsRoutine(flashInterval);
            }
        }

        RestoreOriginalColor();
        flashCoroutine = null;
    }

    private IEnumerator WaitForSecondsRoutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void StopFlash()
    {
        if (flashCoroutine == null)
        {
            return;
        }

        StopCoroutine(flashCoroutine);
        flashCoroutine = null;
    }

    private void RestoreOriginalColor()
    {
        if (targetSprite == null || !hasCachedOriginalColor)
        {
            return;
        }

        targetSprite.color = originalColor;
    }

    public void OnTakenFromPool()
    {
        RestoreOriginalColor();
    }

    public void OnReturnedToPool()
    {
        StopFlash();
        RestoreOriginalColor();
    }

    public void ResetRuntimeState()
    {
        StopFlash();
        RestoreOriginalColor();
    }
}