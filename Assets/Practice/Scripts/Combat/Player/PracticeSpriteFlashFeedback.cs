using System.Collections;
using UnityEngine;

public class PracticeSpriteFlashFeedback
    : HaiMonoBehaviour
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

    private Color originalColor;
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
        if (targetSprite == null)
        {
            return;
        }

        originalColor =
            targetSprite.color;
    }

    public void PlayFlash()
    {
        if (targetSprite == null)
        {
            return;
        }

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
            targetSprite.color =
                flashColor;

            if (flashInterval > 0f)
            {
                yield return new WaitForSeconds(
                    flashInterval
                );
            }

            targetSprite.color =
                originalColor;

            if (flashInterval > 0f)
            {
                yield return new WaitForSeconds(
                    flashInterval
                );
            }
        }

        RestoreOriginalColor();

        flashCoroutine = null;
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
        if (targetSprite == null)
        {
            return;
        }

        targetSprite.color =
            originalColor;
    }
}