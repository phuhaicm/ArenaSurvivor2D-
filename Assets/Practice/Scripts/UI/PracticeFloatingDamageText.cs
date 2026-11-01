using TMPro;
using UnityEngine;

public class PracticeFloatingDamageText : HaiMonoBehaviour, IPracticePoolable
{
    [Header("References")]
    [SerializeField]
    private TMP_Text damageText;

    [Header("Motion & Fade Settings")]
    [SerializeField, Min(0.1f)]
    private float floatUpSpeed = 1.6f;

    [SerializeField, Min(0.1f)]
    private float lifeTime = 0.6f;

    [SerializeField]
    private Color textColor = new Color(1f, 0.92f, 0.23f, 1f);

    private float elapsedTime;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadDamageText();
    }

    protected override void Awake()
    {
        base.Awake();
        LoadDamageText();
    }

    private void LoadDamageText()
    {
        if (damageText != null)
        {
            return;
        }

        damageText = GetComponentInChildren<TMP_Text>(true);
    }

    public void Initialize(float damageAmount, Vector3 worldPosition)
    {
        ResetRuntimeState();

        // Lệch nhẹ ngẫu nhiên trên trục X để các số sát thương liên tiếp không đè lên nhau
        float randomOffsetX = Random.Range(-0.25f, 0.25f);
        transform.position = worldPosition + new Vector3(randomOffsetX, 0.45f, 0f);

        if (damageText != null)
        {
            int displayDamage = Mathf.Max(1, Mathf.RoundToInt(damageAmount));
            damageText.text = displayDamage.ToString();
            damageText.color = textColor;
        }
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        transform.position += Vector3.up * (floatUpSpeed * Time.deltaTime);

        if (damageText != null)
        {
            float normalizedTime = Mathf.Clamp01(elapsedTime / lifeTime);
            Color currentColor = damageText.color;
            currentColor.a = 1f - normalizedTime;
            damageText.color = currentColor;
        }

        if (elapsedTime >= lifeTime)
        {
            PracticeObjectPoolManager.DespawnOrDestroy(gameObject);
        }
    }

    public void OnTakenFromPool()
    {
        ResetRuntimeState();
    }

    public void OnReturnedToPool()
    {
        ResetRuntimeState();
    }

    public void ResetRuntimeState()
    {
        elapsedTime = 0f;

        if (damageText != null)
        {
            Color c = textColor;
            c.a = 1f;
            damageText.color = c;
        }
    }
}