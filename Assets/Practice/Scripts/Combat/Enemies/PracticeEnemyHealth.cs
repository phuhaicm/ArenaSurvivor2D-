using System;
using UnityEngine;

public class PracticeEnemyHealth : PracticeHealthBase
{
    //Enemy có một sự kiện tên Died.
//Script khác có thể đăng ký nghe sự kiện này.
//Khi enemy chết, enemy sẽ phát event này.
    protected override void Die()
    {
        base.Die();

        Destroy(gameObject);
    }
}