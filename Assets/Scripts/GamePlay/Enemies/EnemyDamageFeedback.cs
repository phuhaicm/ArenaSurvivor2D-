using System.Collections;
using UnityEngine;

public class EnemyDamageFeedback : HaiMonoBehaviour
{
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private Color flashColor = Color.white;

    private EnemyHealth enemyHealth;
    private SpriteRenderer spriteRenderer;
    private DamageTextContainerRoot damageTextContainerRoot;
    private Color originalColor;
    private Coroutine flashRoutine;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadEnemyHealth();
        LoadSpriteRenderer();
        LoadDamageTextContainerRoot();
    }

    protected override void Awake()
    {
        base.Awake();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (enemyHealth != null)
        {
            enemyHealth.Damaged += HandleDamaged;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (enemyHealth != null)
        {
            enemyHealth.Damaged -= HandleDamaged;
        }
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        flashDuration = 0.08f;
        flashColor = Color.white;
    }

    private void LoadEnemyHealth()
    {
        if (enemyHealth != null) return;
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void LoadSpriteRenderer()
    {
        if (spriteRenderer != null) return;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LoadDamageTextContainerRoot()
    {
        if (damageTextContainerRoot != null) return;
        damageTextContainerRoot = FindFirstObjectByType<DamageTextContainerRoot>();
    }

    private void HandleDamaged(int damageAmount)
    {
        PlayFlash();
        SpawnDamageText(damageAmount);
    }

    private void PlayFlash()
    {
        if (spriteRenderer == null) return;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
        flashRoutine = null;
    }

    private void SpawnDamageText(int damageAmount)
    {
        FloatingDamageText prefab = ResourcePrefabLoader.LoadPrefab<FloatingDamageText>(GameResourcePaths.FloatingDamageText);
        if (prefab == null)
        {
            Debug.LogWarning("FloatingDamageText prefab not found. Check Resources path.");
            return;
        }

        Vector3 spawnPosition = transform.position + Vector3.up * 0.45f;
        Transform parent = damageTextContainerRoot != null ? damageTextContainerRoot.transform : null;

        FloatingDamageText damageText = Instantiate(prefab, spawnPosition, Quaternion.identity, parent);
        damageText.SetDamageValue(damageAmount);
    }
}