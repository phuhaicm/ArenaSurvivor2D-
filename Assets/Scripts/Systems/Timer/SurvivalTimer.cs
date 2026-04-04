
using UnityEngine;

public class SurvivalTimer : HaiMonoBehaviour
{
    [SerializeField] private float elapsedTime;

    public float ElapsedTime => elapsedTime;

    protected override void ResetValues()
    {
        base.ResetValues();
        elapsedTime = 0f;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        elapsedTime += Time.deltaTime;
    }
}
