using UnityEngine;

[CreateAssetMenu(
    fileName = "PracticeAudioLibrary",
    menuName = "Practice/Audio/Audio Library"
)]
public class PracticeAudioLibrary : ScriptableObject
{
    [Header("Background Music (BGM)")]
    [SerializeField]
    private AudioClip menuBgm;

    [SerializeField]
    private AudioClip gameplayBgm;

    [SerializeField]
    private AudioClip bossBgm;

    [Header("Gameplay SFX")]
    [SerializeField]
    private AudioClip shootSfx;

    [SerializeField]
    private AudioClip enemyDeathSfx;

    [SerializeField]
    private AudioClip playerHitSfx;

    [SerializeField]
    private AudioClip levelUpSfx;

    [Header("UI & Result SFX")]
    [SerializeField]
    private AudioClip buttonClickSfx;

    [SerializeField]
    private AudioClip upgradePickSfx;

    [SerializeField]
    private AudioClip bossWarningSfx;

    [SerializeField]
    private AudioClip gameOverSfx;

    [SerializeField]
    private AudioClip victorySfx;

    public AudioClip MenuBgm => menuBgm;
    public AudioClip GameplayBgm => gameplayBgm;
    public AudioClip BossBgm => bossBgm != null ? bossBgm : gameplayBgm;

    public AudioClip ShootSfx => shootSfx;
    public AudioClip EnemyDeathSfx => enemyDeathSfx;
    public AudioClip PlayerHitSfx => playerHitSfx;
    public AudioClip LevelUpSfx => levelUpSfx != null ? levelUpSfx : upgradePickSfx;

    public AudioClip ButtonClickSfx => buttonClickSfx;
    public AudioClip UpgradePickSfx => upgradePickSfx != null ? upgradePickSfx : levelUpSfx;
    public AudioClip BossWarningSfx => bossWarningSfx;
    public AudioClip GameOverSfx => gameOverSfx;
    public AudioClip VictorySfx => victorySfx != null ? victorySfx : levelUpSfx;
}