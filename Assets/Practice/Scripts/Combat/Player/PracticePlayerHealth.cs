using System;
using UnityEngine;

public class PracticePlayerHealth : PracticeHealthBase   
{
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 0.5f;
    private float invulnerabilityEndTime;
    //public event Action Died;
    public event Action<float, float> HealthChanged;
    public event Action<float> damageTaken;
    private bool isInvulnerable;// về mặt kĩ thuật là ko cần biến này nhưng muốn property rõ nghĩa và bool và tgian có thể mất đồng bộ
    public bool IsInvulnerable => Time.time < invulnerabilityEndTime;// property này sẽ trả về true nếu player đang trong trạng thái bất tử, false nếu ko
    public override void TakeDamage(float damageAmount)//như giờ player còn 5 máu nhưng lại nhận 10 dame thì nó sẽ chết và hp = 0, ko còn âm
    {
        if(isDead) return;
        if(damageAmount <= 0f) return;
        if(IsInvulnerable) return;

        float previousHealth = currentHealth;

        base.TakeDamage(damageAmount);

        float actualDamageTaken = previousHealth - currentHealth;

        if(actualDamageTaken <= 0f) return;

        invulnerabilityEndTime = Time.time + invulnerabilityDuration;

        HealthChanged?.Invoke(currentHealth, maxHealth);
        damageTaken?.Invoke(actualDamageTaken);// cái này tbao là player vừa nhận dmg, có thể dùng để trigger animation hoặc hiệu ứng
    }

    protected override void Die()
    {
        base.Die();
        Debug.Log("Game Over");
    }

}
