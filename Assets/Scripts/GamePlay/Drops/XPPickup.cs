using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class XPPickup : HaiMonoBehaviour
{
    [SerializeField] private int experienceValue = 10;

    public int ExperienceValue => experienceValue;

    protected override void ResetValues()
    {
        base.ResetValues();
        experienceValue = 10;
    }

    public void SetExperienceValue(int value)
    {
        experienceValue = Mathf.Max(1, value);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!TryGetPlayerExperience(other, out PlayerExperience playerExperience))
        {
            return;
        }

        playerExperience.GainExperience(experienceValue);
        gameObject.SetActive(false);
    }

    private bool TryGetPlayerExperience(Collider2D other, out PlayerExperience playerExperience)
    {
        playerExperience = other.GetComponentInParent<PlayerExperience>();
        return playerExperience != null;
    }
}