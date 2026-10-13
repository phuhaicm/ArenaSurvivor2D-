using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PracticePlayerHealth))]
[RequireComponent(typeof(SpriteRenderer))]
public class PracticePlayerDamageFeedback : HaiMonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField, Min(1f)] private int flashCount = 3;
    [SerializeField, Min(0f)] private float flashInterval = 0.08f;
    [SerializeField] private Color flashColor = Color.red;

    //ko dùng update vì ko cần, chỉ cần trigger khi player nhận dmg, ko cần check liên tục và làm giảm hiệu năng
    private PracticePlayerHealth playerHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadPlayerHealth();
        this.LoadSpriteRenderer();
    }
    private void LoadPlayerHealth()
    {
        if(this.playerHealth != null) return;
        this.playerHealth = GetComponent<PracticePlayerHealth>();
    }
    private void LoadSpriteRenderer()
    {
        if(this.spriteRenderer != null) return;
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        this.originalColor = this.spriteRenderer.color;
    }
    protected override void OnEnable()
    {
        this.playerHealth.damageTaken += this.HandleDamageTaken;
    }
    //couroutine sẽ chạy trong 1 frame, nên khi player chết và respawn thì nó vẫn chạy tiếp, dẫn đến flash nhiều lần, nên cần unsubscribe khi disable. Courountine là 1 method chạy 1 đoạn dừng tại yield chờ 1 điều kiện hoặc thời gian, tiếp tục chạy tại chỗ đã dừng
    protected override void OnDisable()
    {
        this.playerHealth.damageTaken -= this.HandleDamageTaken;//ko unsubscribe thì khi player chết và respawn thì sẽ bị subscribe nhiều lần, dẫn đến flash nhiều lần
        if(this.flashCoroutine != null)
        {
            StopCoroutine(this.flashCoroutine);// nếu player chết và respawn thì couroutine vẫn chạy tiếp, nên cần stop nó khi disable
            this.flashCoroutine = null;// reset couroutine để tránh bị null reference exception khi player respawn và nhận dmg tiếp
        }
        if(this.spriteRenderer != null)
        {
            this.spriteRenderer.color = this.originalColor;
        }
    }
    private void HandleDamageTaken(float damageAmount)// hiện tại chưa dùng damageAmount, nhưng có thể dùng để thay đổi flashColor dựa trên lượng dmg nhận vào
    {
        if(this.flashCoroutine != null)
        {
            StopCoroutine(this.flashCoroutine);// nếu player nhận dmg liên tục thì couroutine sẽ chạy nhiều lần, nên cần stop couroutine trước khi start lại
            flashCoroutine = null;// reset couroutine để tránh bị null reference exception khi player nhận dmg liên tục
        }
        if(this.spriteRenderer != null)
        {
            this.spriteRenderer.color = this.originalColor;
        }
        this.flashCoroutine = StartCoroutine(this.FlashCoroutine());
    }
    private IEnumerator FlashCoroutine()
    {
        for (int i = 0; i < this.flashCount; i++)
        {
            this.spriteRenderer.color = this.flashColor;
            yield return new WaitForSeconds(this.flashInterval);
            this.spriteRenderer.color = this.originalColor;
            yield return new WaitForSeconds(this.flashInterval);
        }
        spriteRenderer.color = this.originalColor;// đảm bảo rằng màu sắc của sprite được reset về màu gốc sau khi flash kết thúc
        this.flashCoroutine = null;//phải đặt null vì đã kết thúc courountine  
    }
}

