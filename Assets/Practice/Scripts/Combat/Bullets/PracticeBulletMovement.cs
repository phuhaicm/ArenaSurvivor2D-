using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PracticeBulletMovement : HaiMonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;

    private Vector2 moveDirection;

    protected override void ResetValues()
    {
        base.ResetValues();
        moveSpeed = 12f;
    }
    private void Update()
    {
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
        //delta time là thời gian chạy giữa các frame và hàm này phải nhân với deltatime để đồng bộ mỗi frame bullet đi
    }
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        //phải dùng normalized để quy chuẩn về thành 1 độ dài tránh trường hợp càng xa thì đạn đi càng nhanh
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        // dòng này là tính góc atan2 thì trả về radian nên phải nhân thêm để chuyển về độ
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        //2 dòng code này là để xoay hình viên đạn theo hướng cần bay
    }



}
