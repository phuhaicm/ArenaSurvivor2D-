using UnityEngine;

public class PracticeXPAttractable : HaiMonoBehaviour
{
    [Header("Attraction Settings")]
    [SerializeField, Min(0f)] private float initalAttractionSpeed = 3f;
    [SerializeField, Min(0f)] private float attractionAcceleration = 8f;
    [SerializeField, Min(0f)] private float maxiumAttractionSpeed = 11f;
    private float currentAttractionSpeed;
    private Transform attractionTarget;
    public Transform AttractionTarget => attractionTarget;
    public bool HasAttractionTartget => attractionTarget != null;
    protected override void Awake() {
        base.Awake();

        currentAttractionSpeed = initalAttractionSpeed;
    }
    private void Update()// nguyên tắc transform movement là update + deltatime còn rigibody physics movement thì fixupdate + fixedeltatime
    {
        if(!HasAttractionTartget) return;

        UpdateAttractionSpeed();
        MoveTowardsTarget();
    }
    private void UpdateAttractionSpeed()
    {
        currentAttractionSpeed =
        Mathf.MoveTowards(currentAttractionSpeed, maxiumAttractionSpeed, attractionAcceleration * Time.deltaTime);//tăng tốc độ hút theo mỗi frame. Hàm này làm việc với số
    }
    public void SetAttractionTarget(Transform newTarget)
    {
        if(newTarget == null) return;

        if(attractionTarget != null) return;

        attractionTarget = newTarget;

        Debug.Log(
            $"{name}: đã nhận attraction target: " + $"{attractionTarget.name}",this
        );
        currentAttractionSpeed = initalAttractionSpeed;// để có thể tái sử dụng
    }
    private void MoveTowardsTarget()
    {
        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = attractionTarget.position;

        float movementThisFrame = currentAttractionSpeed * Time.deltaTime;
        Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, movementThisFrame);//sử dụng hàm này thay vì tự tính direction vì khi khoảng cách giữa player và orb nhỏ hơn quãng đường mà frame muốn đi thì orb sẽ vượt qua player và sau đó mới quay đầu lại nên sử dụng hàm này thay vì tự tính. Hàm này làm việc với tọa độ

        transform.position = newPosition;
    }
    private void CleanAttractionTarget()
    {
        attractionTarget = null;

        currentAttractionSpeed = initalAttractionSpeed;
    }


}
