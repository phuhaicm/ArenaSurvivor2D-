using UnityEngine;

public class PracticeXPAttractable : HaiMonoBehaviour, IPracticePoolable
{
    [Header("Attraction Settings")]
    [SerializeField, Min(0f)] private float initalAttractionSpeed = 3f;
    [SerializeField, Min(0f)] private float attractionAcceleration = 8f;
    [SerializeField, Min(0f)] private float maxiumAttractionSpeed = 11f;

    private float currentAttractionSpeed;
    private Transform attractionTarget;

    public Transform AttractionTarget => attractionTarget;
    public bool HasAttractionTartget => attractionTarget != null;

    protected override void Awake()
    {
        base.Awake();
        currentAttractionSpeed = initalAttractionSpeed;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        CleanAttractionTarget();
    }

    protected override void OnDisable()
    {
        CleanAttractionTarget();
        base.OnDisable();
    }

    private void Update()
    {
        if (!HasAttractionTartget) return;

        UpdateAttractionSpeed();
        MoveTowardsTarget();
    }

    private void UpdateAttractionSpeed()
    {
        currentAttractionSpeed =
            Mathf.MoveTowards(
                currentAttractionSpeed,
                maxiumAttractionSpeed,
                attractionAcceleration * Time.deltaTime
            );
    }

    public void SetAttractionTarget(Transform newTarget)
    {
        if (newTarget == null) return;
        if (attractionTarget != null) return;

        attractionTarget = newTarget;
        currentAttractionSpeed = initalAttractionSpeed;
    }

    private void MoveTowardsTarget()
    {
        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = attractionTarget.position;

        float movementThisFrame = currentAttractionSpeed * Time.deltaTime;
        Vector2 newPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            movementThisFrame
        );

        transform.position = newPosition;
    }

    private void CleanAttractionTarget()
    {
        attractionTarget = null;
        currentAttractionSpeed = initalAttractionSpeed;
    }

    public void OnTakenFromPool()
    {
        CleanAttractionTarget();
    }

    public void OnReturnedToPool()
    {
        CleanAttractionTarget();
    }

    public void ResetRuntimeState()
    {
        CleanAttractionTarget();
    }
}