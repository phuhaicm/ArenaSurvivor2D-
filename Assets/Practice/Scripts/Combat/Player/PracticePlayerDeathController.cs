using UnityEngine;

public class PracticePlayerDeathController : HaiMonoBehaviour
{
    [Header("Reference")]
    [SerializeField] protected PracticePlayerHealth playerHealth;
    [SerializeField] protected Rigidbody2D rigibody2D;
    [Header("Player Behaviour")]
    [SerializeField] protected Behaviour[] behavioursToDisable;
    protected bool isPlayerDead;
    public bool IsPlayerDead => isPlayerDead;
    protected override void Start()
    {
        base.Start();

        isPlayerDead = false;
        SetPlayerBehavioursEnabled(true);
    }
    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadPlayerHealth();
        LoadRigibody2D();
    }
    protected void LoadPlayerHealth()
    {
        if(playerHealth != null) return;

        playerHealth = GetComponent<PracticePlayerHealth>();
    }
    protected void LoadRigibody2D()
    {
        if(rigibody2D != null) return;

        rigibody2D = GetComponent<Rigidbody2D>();//dùng getcomponent thay vì dùng firstfind vì dead controller thuộc về 1 player cụ thể
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribePlayerEvents();
    }
    protected override void OnDisable()
    {
        UnSubscribePlayerEvents();
        base.OnDisable();
    }
    protected void SubscribePlayerEvents()
    {
        if(playerHealth == null) return;
        playerHealth.Died += HandlePlayerDied;
    }
    protected void UnSubscribePlayerEvents()
    {
        if(playerHealth == null) return;
        playerHealth.Died -= HandlePlayerDied;
    }
    protected void HandlePlayerDied()
    {
        if(isPlayerDead) return;
        isPlayerDead = true;
        SetPlayerBehavioursEnabled(false);
        StopMovement();
    }
    protected void SetPlayerBehavioursEnabled(bool enabledState)
    {
        if(behavioursToDisable == null) return;
        
        foreach(Behaviour behaviour in behavioursToDisable)
        {
            if(behaviour == null) return;
            if(behaviour == this) return;//this ở đây chính là component deadcontroller thêm dòng này để chặn lỗi tự thêm chính mình

            behaviour.enabled = enabledState;
        }
    }
    protected void StopMovement()
    {
        if(rigibody2D == null) return;

        rigibody2D.velocity = Vector2.zero;//bản này dùng velocity chứ ko dùng linearVelocity
        rigibody2D.angularVelocity = 0f;
    }
} 
