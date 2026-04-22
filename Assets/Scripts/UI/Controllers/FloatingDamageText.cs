using TMPro;
using UnityEngine;

public class FloatingDamageText : HaiMonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float lifetime = 0.8f;

    private TextMeshPro textMesh;
    private Color baseColor;
    private float elapsedTime;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadTextMesh();
    }

    protected override void ResetValues()
    {
        base.ResetValues();
        moveSpeed = 1.2f;
        lifetime = 0.8f;
    }

    private void LoadTextMesh()
    {
        if (textMesh != null) return;
        textMesh = GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        elapsedTime += Time.deltaTime;
        float normalized = Mathf.Clamp01(elapsedTime / lifetime);

        if (textMesh != null)
        {
            Color currentColor = baseColor;
            currentColor.a = 1f - normalized;
            textMesh.color = currentColor;
        }

        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetDamageValue(int damage)
    {
        if (textMesh == null)
        {
            LoadTextMesh();
        }

        if (textMesh != null)
        {
            textMesh.text = damage.ToString();
            baseColor = textMesh.color;
        }
    }
}