HỒ SƠ CHUYỂN GIAO DỰ ÁN
Project Handover Document

Tên dự án: 2D Arena Survivor / Top-down Auto-shooter
 Chủ dự án: Lê Nguyễn Phú Hải
 Công nghệ: Unity, C#
 Mục tiêu cá nhân: Hoàn thiện một game có kiến trúc đủ mạnh về kỹ thuật để phục vụ mục tiêu ứng tuyển thực tập Game Developer
 Codebase chính: production
 Phiên bản học tập: practice-learning
 Source cũ: main, chỉ lưu làm tài liệu tham khảo và khôi phục
 Trạng thái tài liệu: Tổng hợp đến giai đoạn hoàn thành Boss Radial Attack và chuẩn bị triển khai Boss Phase / Attack Selection

Lưu ý quan trọng về độ chính xác:
 Tài liệu này phân biệt rõ:

Đã hoàn thành và đã test: những hệ thống đã được xác nhận hoạt động.
Đang hoàn thiện: chức năng đã chạy nhưng còn cần review, polish hoặc kiểm thử edge case.
Đã thiết kế nhưng chưa xác nhận triển khai: kiến trúc/code đã được đề xuất nhưng chưa có xác nhận rằng đã setup và test thành công trong Unity.

Cụ thể, Boss Radial Attack đã được người phát triển xác nhận làm xong, còn Boss Phase / Attack Selection mới ở trạng thái thiết kế và hướng dẫn triển khai, chưa được xem là feature hoàn thành.

1. Tổng quan Dự án
1.1. Thể loại game

Dự án là game:

2D.
Top-down.
Arena Survivor.
Auto-shooter.
Chơi theo từng run.
Người chơi tập trung vào di chuyển, né tránh, thu thập XP và lựa chọn nâng cấp.
Hệ thống Shooter tự động tìm mục tiêu và bắn.
Độ khó tăng dần theo thời gian.
Cuối run chuyển sang Final Boss.
Đánh bại Final Boss để hoàn thành run.

Định hướng không dừng ở mức prototype hoặc MVP đơn giản. Game cuối cùng phải đáp ứng đồng thời:

Gameplay loop hoàn chỉnh.
Kiến trúc có khả năng mở rộng.
Data-driven.
Object Pooling.
JSON persistence.
UI/Audio/VFX hoàn chỉnh.
Performance profiling.
Benchmark và stress test.
Windows build có thể phát hành.
Tài liệu kiến trúc và báo cáo hiệu năng.
1.2. Lịch sử codebase và quyết định kiến trúc

Dự án ban đầu chứa một source game cũ được xây dựng trong quá trình học từ tutorial hoặc người có kinh nghiệm.

Sau đó, toàn bộ gameplay được viết lại trong khu vực Practice nhằm:

Hiểu sâu hơn về trách nhiệm của từng script.
Tự xây dựng flow thay vì phụ thuộc code có sẵn.
Học cách chia module.
Hiểu event, lifecycle và dependency trong Unity.
Giảm tình trạng sao chép code mà không nắm được kiến trúc.
Chuẩn bị một codebase đủ sạch để tiếp tục phát triển thành sản phẩm hoàn chỉnh.

Lộ trình Practice đã hoàn thành từ Bài 1 đến Bài 63.

Sau Bài 63, quyết định kỹ thuật chính thức là:

Không tiếp tục migration toàn bộ source cũ.
Không đối chiếu toàn bộ Practice với Arena legacy.
Không tiếp tục phát triển source legacy.
Dùng Practice tự viết làm codebase chính.
Source cũ chỉ lưu trên Git/GitHub để tham khảo hoặc khôi phục.
Chuyển Practice sang giai đoạn Production.
Các milestone production không được coi là tùy chọn.
1.3. Cấu trúc Git hiện tại
Các branch dài hạn
main
└── Source game cũ/legacy
    Không tiếp tục phát triển

practice-learning
└── Bản Practice hoàn thành Bài 1–63
    Giữ làm mốc học tập

production
└── Codebase chính để phát triển game hoàn chỉnh
Branch tính năng
feature/*
└── Tách từ production
    Code và setup một tính năng cụ thể
    Merge trở lại production bằng Pull Request

    Workflow bắt buộc
    production
→ tạo feature branch
→ code
→ setup Unity
→ runtime test
→ kiểm tra Console
→ commit
→ push
→ Pull Request, base là production
→ merge
→ quay lại production
→ pull
→ xóa feature branch local và remote
Nguyên tắc quan trọng
Không merge main vào production.
Không tiếp tục code production trên main.
Không mặc định trạng thái branch.
Luôn kiểm tra bằng:
Shell
git branch --show-current
git status --short
Không push feature trước khi Unity setup và runtime test hoàn tất.
Không dùng git reset --hard hoặc git clean khi chưa hiểu rõ thay đổi đang tồn tại.
Mỗi feature nên có commit và Pull Request riêng.
1.4. Core mechanics
Di chuyển Player
Player di chuyển trong không gian top-down 2D.
Input điều khiển movement.
Player chủ yếu chịu trách nhiệm:
Điều hướng.
Né projectile.
Tiếp cận pickup.
Sống sót.
Shooter hoạt động như module chiến đấu thuộc Player.
Auto-target và auto-shoot
Shooter tự tìm Enemy hợp lệ.
Tự bắn theo attack interval.
Bullet di chuyển và gây damage.
Các thông số có thể được nâng cấp:
Bullet Damage.
Fire Rate.
Projectile Count.
Enemy combat
Enemy spawn theo thời gian.
Enemy đuổi theo Player.
Enemy gây contact damage.
Enemy có health và damage feedback.
Khi chết:
Phát event chết.
Cập nhật kill count/score.
Rơi XP Orb.
Bị hủy hoặc sau này trả về pool.
XP và Level

Flow progression hiện tại:

Plain Text
Enemy chết
→ rơi XP Orb
→ Player nhặt XP
→ PracticePlayerExperience nhận XP
→ phát ExperienceGained
→ PracticePlayerLevelSystem xử lý XP tiến độ
→ kiểm tra điều kiện Level Up
→ phát ExperienceProgressChanged
→ phát LeveledUp
→ Upgrade Choice xuất hiện

Kiến trúc XP/Level từng được chỉnh lại để tránh PracticePlayerExperience và PracticePlayerLevelSystem cùng quản lý Level. PracticePlayerExperience chỉ nhận và phát XP; PracticePlayerLevelSystem giữ level, XP trong level hiện tại, required XP và phát các event progression.
Upgrade

Khi lên level:

Plain Text
LeveledUp
→ Game State chuyển UpgradeChoice
→ Time.timeScale = 0
→ Upgrade UI hiển thị lựa chọn
→ người chơi chọn upgrade, reroll hoặc skip
→ upgrade được áp dụng
→ UI đóng
→ Game State quay lại Running
→ Time.timeScale = 1

Một lỗi quan trọng trước đây là sau khi chọn upgrade, UI request lại UpgradeChoice, khiến game tiếp tục ở Time.timeScale = 0. Lỗi đã được xác định và sửa bằng cách request trở về Running.

Survivor run
Run bắt đầu ở phase Survival.
Enemy thường được spawn.
Difficulty tăng dần.
Survival Timer theo dõi thời gian.
Nếu Player chết:
Game chuyển GameOver.
Nếu Player sống đủ Run Duration:
Không Victory trực tiếp nữa.
Chuyển sang BossFight.
Dừng spawn Enemy thường.
Spawn Final Boss.
Boss Combat
Plain Text
Survival kết thúc
→ PracticeRunDirector chuyển BossFight
→ FinalBossController spawn Final Boss
→ Boss Warning UI xuất hiện
→ Boss Health Panel xuất hiện
→ Boss thực hiện attack
→ Boss nhận damage
→ Boss chết
→ Run phase chuyển Completed
→ Game State chuyển Victory
→ Victory Result UI xuất hiện
Kết quả run

Victory Result UI hiển thị:

Survival Time.
Kill Count.
Final Score.
Best Score.
Restart.
Main Menu.
1.5. Game loop tổng thể
Plain Text
Main Menu
→ người chơi chọn Play
→ Async Loading
→ Loading Progress
→ Fade Out/Fade In
→ Gameplay Scene
→ Game State = Running
 
→ Player di chuyển
→ Shooter tự tìm Enemy
→ Shooter tự bắn
→ Bullet gây damage
→ Enemy chết
→ XP Orb xuất hiện
→ Player nhặt XP
→ Level System tăng progression
→ Player Level Up
→ Upgrade Choice
→ nâng cấp Shooter hoặc module phù hợp
→ quay lại Running
 
→ Difficulty tăng dần
→ Enemy spawn nhiều/khó hơn
→ Kill Count và Score tăng
 
├── Player chết
│ → Game Over
│ → Result/Game Over UI
│ → Restart hoặc Main Menu
│
└── sống đủ Run Duration
→ BossFight
→ dừng spawn thường
→ Final Boss xuất hiện
→ Boss Warning
→ Boss Health UI
→ Boss Combat
→ Boss chết
→ Victory
→ Victory Result UI
→ Restart hoặc Main Menu
2. Các Tính năng & Module ĐÃ HOÀN THÀNH
2.1. Core foundation
HaiMonoBehaviour

Base class dùng chung cho nhiều MonoBehaviour trong project.

Trách nhiệm dự kiến/đã áp dụng:

Chuẩn hóa lifecycle.
Cung cấp các hook như:
LoadComponents().
ResetValues().
Awake().
OnEnable().
OnDisable().
Giảm việc lặp logic tự load component.
Giúp Inspector reference tự động được resolve khi phù hợp.
Cho phép derived class gọi base để giữ lifecycle nhất quán.
IPracticeDamageable

Interface biểu diễn một đối tượng có thể nhận damage.

Mục đích:

Tách damage dealer khỏi class health cụ thể.
Cho phép projectile hoặc contact damage gọi chung một contract.
Hạn chế coupling trực tiếp với từng loại Player/Enemy.
PracticeHealthBase

Base class sức khỏe dùng chung.

Dữ liệu và API chính:

maxHealth.
currentHealth.
isDead.
CurrentHealth.
MaxHealth.
IsDead.
TakeDamage(float damage).
damageTaken.
Died.

Flow:

Plain Text
TakeDamage
→ guard: đã chết thì bỏ qua
→ guard: damage <= 0 thì bỏ qua
→ giảm health bằng Mathf.Clamp
→ tính applied damage thật
→ phát damageTaken
→ nếu health <= 0
→ Die
→ isDead = true
→ phát Died
 

Điểm tốt:

Có guard clause.
Damage event chỉ phát khi HP thật sự giảm.
Died chỉ phát một lần nhờ isDead.
Derived class có thể override Die().

Điểm cần chú ý:

Event đặt tên damageTaken viết thường không theo convention C# tiêu chuẩn. Về sau nên cân nhắc đổi thành DamageTaken.
Chưa có HealthChanged(current, max) chuyên biệt.
Chưa có API reset health cho Object Pooling.
Chưa có heal/max health modifier chính thức.
PracticePlayerHealth

Health dành cho Player.

Trách nhiệm:

Kế thừa health base.
Nhận damage từ Enemy hoặc Boss Projectile.
Quản lý invulnerability.
Phát sự kiện chết để Game State chuyển Game Over.
Tích hợp damage feedback cho Player.
PracticeEnemyHealth

Health dành cho Enemy và Boss.

Flow chết hiện tại:

C#
protected override void Die()
{
base.Die();
Destroy(gameObject);
}

Ý nghĩa:

Gọi base.Die() trước.
isDead = true.
Phát Died.
Các listener cleanup/ghi nhận kill/Victory.
Sau đó mới Destroy(gameObject).

Thứ tự này hợp lý vì các listener nhận được event trước khi object bị hủy.

Hạn chế hiện tại:

Dùng Destroy, chưa sẵn sàng cho Enemy Pooling.
Chưa có phương thức reset health public/protected dành cho pooled object.
Boss và Enemy thường đang dùng chung lớp health, điều này ổn nếu behavior chết khác nhau được điều phối bằng listener.
2.2. Game State
PracticeGameState

Enum trạng thái gameplay.

Các state hiện có:

Running.
Paused.
UpgradeChoice.
GameOver.
Victory.

Enum giúp:

Tránh magic string.
Dễ kiểm soát transition.
Cho UI và gameplay cùng đọc một nguồn trạng thái.
PracticeGameStateController

Owner trung tâm của trạng thái gameplay.

Trách nhiệm:

Giữ current state.
Cung cấp request methods:
Request Running.
Request Pause.
Request Upgrade Choice.
Request Game Over.
Request Victory.
Quản lý Time.timeScale.
Phát StateChanged.
Cung cấp các property kiểm tra:
IsRunning.
IsPaused.
IsChoosingUpgrade.
IsGameOver.
IsVictory.

Nguyên tắc kiến trúc:

Plain Text
Gameplay/UI gửi request
→ GameStateController đổi state
→ phát StateChanged
→ UI và subsystem đồng bộ theo state

Không nên để từng UI tự tùy tiện quyết định Time.timeScale.

State-driven UI đã được sử dụng để đồng bộ Pause, Upgrade và Game Over panel, tránh nhiều panel không tương thích cùng xuất hiện.

2.3. Scene Flow
PracticeSceneLoader

Trách nhiệm:

Chuyển scene.
Async loading.
Theo dõi progress.
Điều phối Fade Out/Fade In.
Khôi phục Time.timeScale khi chuyển scene.
Hỗ trợ:
Play từ Main Menu.
Restart.
Quay lại Main Menu.
SceneLoaderRoot

Persistent root chứa Scene Loader.

Mục tiêu:

Không bị hủy trong quá trình chuyển scene.
Tránh mỗi scene tạo một loader riêng.
Quản lý transition tập trung.
Main Menu

Đã có flow:

Plain Text
Main Menu
→ Play
→ Loading
→ Gameplay Scene

Main Menu từng được xây dựng để khắc phục việc bấm Play trong Unity là gameplay chạy ngay, tạo một điểm bắt đầu đúng cho vòng đời game.

Async Loading

Đã xử lý progress của Unity, bao gồm đặc điểm:

Plain Text
loadOperation.progress đạt tối đa khoảng 0.9 trước activation
→ chia cho 0.9
→ Mathf.Clamp01
→ hiển thị thành progress 0–1
Fade Transition

Có:

Fade Out trước scene change.
Fade In khi scene mới sẵn sàng.
Điều phối để gameplay không lộ ra đột ngột.
Khôi phục time scale trong transition.
2.4. Player Module
PracticePlayerMovement

Trách nhiệm:

Đọc input movement.
Tính hướng.
Di chuyển Player.
Tôn trọng pause thông qua time scale hoặc game state.
Tách movement khỏi combat.
PracticePlayerDamageFeedback

Trách nhiệm:

Nhận tín hiệu Player bị damage.
Kích hoạt feedback phù hợp.
Không trực tiếp xử lý health calculation.
PracticeSpriteFlashFeedback

Trách nhiệm:

Flash sprite khi nhận damage.
Quản lý màu gốc.
Chạy flash duration.
Cleanup màu khi disable/death.
Sau này phải reset đúng khi object được pooling.
Invulnerability

Player có khoảng bất tử sau khi trúng damage.

Mục tiêu:

Tránh nhận nhiều hit trong một frame.
Chống contact damage hoặc projectile gây damage liên tục.
Tạo nhịp combat công bằng hơn.
2.5. Experience và Level
PracticePlayerExperience

Trách nhiệm chính thức:

Nhận XP.
Validate XP.
Phát lượng XP nhận được qua event.
Không giữ level.
Không xử lý Level Up.
Không quản lý required XP.

Đây là kết quả của quá trình refactor trách nhiệm giữa Experience và Level System.

PracticePlayerLevelSystem

Trách nhiệm:

Nghe XP từ PracticePlayerExperience.
Giữ current level.
Giữ XP trong level hiện tại.
Giữ required XP.
Xử lý một hoặc nhiều Level Up.
Phát ExperienceProgressChanged.
Phát LeveledUp.

Dependency chính:

Plain Text
PracticePlayerExperience
→ ExperienceGained
→ PracticePlayerLevelSystem
├── cập nhật progression
├── ExperienceProgressChanged
└── LeveledUp

File này từng bị xóa nhầm và đã được khôi phục vì Upgrade System phụ thuộc trực tiếp vào LeveledUp.

Experience UI

Trách nhiệm:

Nghe ExperienceProgressChanged.
Cập nhật XP slider/text.
Không tự tính required XP.
Không tự sửa dữ liệu progression.
2.6. Upgrade System
PracticeUpgradeDefinition

ScriptableObject chứa dữ liệu upgrade.

Vai trò:

Identity.
Tên hiển thị.
Mô tả.
Loại upgrade.
Giá trị tăng.
Icon hoặc presentation reference nếu có.
Cho phép dữ liệu upgrade tách khỏi UI và runtime logic.
PracticeShooterUpgradeApplier

Trách nhiệm:

Nhận yêu cầu áp dụng upgrade.
Route hiệu ứng upgrade tới Shooter.
Tách UI lựa chọn khỏi logic sửa Shooter.

Các upgrade đã có:

Bullet Damage.
Fire Rate.
Projectile Count.
PracticeUpgradeChoiceUI

Trách nhiệm:

Nghe Level Up.
Sinh/hiển thị lựa chọn.
Quản lý pending level choices.
Hiện/ẩn panel.
Gửi request state.
Xử lý:
Chọn upgrade.
Reroll.
Skip.
Quay lại Running khi hoàn thành tất cả lựa chọn.
Reroll
Tạo lại lựa chọn.
Có giới hạn theo runtime state nếu đã cấu hình.
Không trực tiếp tăng level hoặc sửa Shooter.
Skip
Bỏ qua lựa chọn hiện tại.
Giảm pending choice.
Khi hết pending choice thì đóng UI và quay lại Running.
Lỗi đã sửa

Bug:

Plain Text
HidePanel
→ gọi RequestUpgradeChoices lần nữa
→ state vẫn là UpgradeChoice
→ Time.timeScale vẫn là 0
→ gameplay đứng yên

Sửa thành:

Plain Text
Nếu đang IsChoosingUpgrade
→ RequestRunning
→ Time.timeScale trở lại 1

2.7. Shooter và Combat
PracticeShooterWithDamage

Trách nhiệm:

Tìm target.
Điều khiển fire interval.
Tạo projectile.
Truyền damage cho projectile.
Hỗ trợ nhiều projectile.
Nhận các upgrade runtime.

Các API từng được sử dụng:

Tăng bullet damage.
Giảm shoot interval.
Tăng projectile count.
PracticeShooterUpgradeApplier

Là adapter/router giữa upgrade selection và Shooter runtime.

Nguyên tắc:

Plain Text
UI chọn Upgrade Definition
→ Upgrade Applier hiểu effect
→ Shooter nhận thay đổi

UI không được sửa trực tiếp field private của Shooter.

PracticeBulletMovement

Trách nhiệm:

Giữ direction/speed.
Di chuyển player bullet.
Không chịu trách nhiệm damage nếu đã có damage dealer.
PracticeBulletDamageDealer

Trách nhiệm:

Phát hiện va chạm target hợp lệ.
Gọi damage contract.
Chống damage lặp nếu cần.
Không chịu trách nhiệm movement.
PracticeBulletLifetime

Trách nhiệm:

Theo dõi lifetime.
Hủy bullet khi hết thời gian.
Sau này chuyển từ Destroy sang ReturnToPool.
2.8. Enemy Module
PracticeEnemyChase

Trách nhiệm:

Tìm/giữ target Player.
Tính direction đến Player.
Di chuyển Enemy.
Không xử lý contact damage.
PracticeEnemyContactDamage

Trách nhiệm:

Phát hiện contact với Player.
Gọi damage lên Player Health.
Quản lý interval hoặc anti-repeat nếu có.
Không xử lý Enemy movement.
PracticeEnemyDamageFeedback

Trách nhiệm:

Nghe Enemy bị damage.
Kích hoạt flash/hit feedback.
Không sửa health.
PracticeEnemySpawner

Trách nhiệm:

Spawn Enemy.
Chọn prefab Enemy.
Chọn spawn point.
Theo dõi spawn interval.
Dừng spawn khi không ở Survival.
Không điều khiển Boss spawn.
PracticeDifficultySystem

Trách nhiệm:

Tăng độ khó theo thời gian/run progression.
Điều chỉnh spawn pressure.
Có thể thay đổi:
Spawn interval.
Số lượng.
Enemy tier.
Về sau phải chuyển thành data-driven và spawn budget.
Enemy prefabs hiện có
Practice_Enemy_Normal.
Practice_Enemy_Fast.
Practice_Enemy_Tank.
2.9. Pickup System
PracticeXPPickup

Trách nhiệm:

Chứa XP value.
Phát hiện Player.
Bị hút về Player trong pickup range/magnet range.
Gọi PracticePlayerExperience.AddExperience.
Sau khi được nhặt thì bị hủy.
Sau này chuyển sang pooling.
XP Magnet

Đã có khả năng hút XP Orb về Player.

Các hướng mở rộng:

Pickup Radius upgrade.
Magnet upgrade.
XP multiplier.
XP tiers.
Reward drop table.
2.10. Run Systems
PracticeSurvivalTimer

Trách nhiệm:

Theo dõi thời gian sống sót.
Phát tiến độ hoặc hoàn thành duration.
Dừng/không cộng thời gian khi gameplay không Running.
Cung cấp dữ liệu cho HUD và result.
PracticeKillCounter

Trách nhiệm:

Theo dõi số Enemy bị tiêu diệt.
Cung cấp dữ liệu cho HUD và score system.
Reset theo run.
PracticeRunScore

Trách nhiệm:

Tính điểm run.
Kết hợp dữ liệu như:
Survival time.
Kill count.
Có thể thêm multiplier sau này.
Cung cấp Final Score.
PracticeBestScore

Trách nhiệm:

Theo dõi best score.
So sánh run mới.
Hiện tại persistence lâu dài chưa hoàn thiện theo JSON architecture cuối cùng.
PracticeRunPhase

Enum phase cấp run:

None.
Survival.
BossFight.
Completed.

Đây là state của vòng run, khác với PracticeGameState.

Ví dụ:

Plain Text
PracticeRunPhase = BossFight
PracticeGameState = Paused

Hai state này có thể cùng tồn tại vì chúng trả lời hai câu hỏi khác nhau:

Run đang ở giai đoạn nào?
Gameplay hiện đang chạy, pause hay hiện modal?
PracticeRunDirector

Owner của run progression.

Trách nhiệm:

Plain Text
Bắt đầu Survival
→ theo dõi Run Duration
→ chuyển BossFight
→ yêu cầu dừng Enemy spawn thường
→ yêu cầu spawn Final Boss
→ theo dõi Boss chết
→ chuyển Completed
→ RequestVictory

Không nên để UI tự quyết định hoàn thành run.

2.11. UI Module
HUD

Đã có nền tảng hiển thị:

Player Health.
XP/Level.
Survival Time.
Kill Count.
Score.
Thông tin gameplay phù hợp.
Pause UI

Trách nhiệm:

Gửi request pause/resume.
Hiện panel theo Game State.
Có thể Restart/Main Menu.
Không tự quản lý gameplay system.
Upgrade UI
Hiện lựa chọn.
Reroll.
Skip.
Đồng bộ với UpgradeChoice.
Game Over UI
Hiện khi state là GameOver.
Cung cấp Restart/Main Menu.
Không trực tiếp xử lý Player death logic.
Victory Result UI

Đã hoàn thành foundation gồm:

Victory Panel.
Survival Time.
Kill Count.
Final Score.
Best Score.
Restart.
Main Menu.
Responsive desktop layout cơ bản.
Settings UI

Đã có foundation để điều khiển audio settings.

Boss Warning UI

Đã hoàn thành:

Banner cảnh báo Boss.
Fade In.
Hold.
Fade Out.
Tách presentation khỏi Boss spawn logic.
Boss Health UI

Đã hoàn thành:

Screen-space cố định.
Boss Name.
Health Slider.
Health Text.
Cập nhật thông qua event damage/death.
Không đi theo Boss.

Đây là chủ ý thiết kế cho Final Boss. Elite/Mini Boss về sau mới cân nhắc world-space health bar.

2.12. Audio Foundation
PracticeAudioRoot

Persistent root của audio system.

Trạng thái:

Đã có foundation.
Chưa hoàn thiện prefab đầy đủ.
Chưa tích hợp đầy đủ AudioClip thật vào toàn game.
PracticeAudioManager

Trách nhiệm dự kiến:

Quản lý Music Source.
Quản lý SFX Source.
Play Music/SFX.
Điều phối volume.
Là điểm truy cập audio tập trung.
PracticeAudioSettings

Trách nhiệm:

Lưu/áp dụng Master/Music/SFX volume.
Giao tiếp với Audio Mixer.
Kết nối Settings UI.
PracticeAudioMixer

Đã có exposed parameters:

Master.
Music.
SFX.
Audio Sources
MusicSource.
SfxSource.
Trạng thái chính xác

Audio chưa được coi là hoàn thành vì:

Chưa có đầy đủ AudioClip thật.
PracticeAudioRoot prefab chưa hoàn thiện toàn bộ.
Chưa đặt/kiểm tra đầy đủ prefab ở tất cả scene cần thiết.
Chưa test đầy đủ Main Menu BGM, Gameplay BGM và Boss BGM.
Chưa tích hợp toàn bộ combat/UI SFX.
2.13. Final Boss Foundation
FinalBossController

Trách nhiệm:

Giữ Boss Prefab.
Giữ Boss Spawn Point.
Giữ Boss Container.
Spawn Boss khi PracticeRunDirector chuyển Boss Fight.
Theo dõi Active Boss.
Không đặt Boss sẵn trong scene ngoài Play Mode.

Setup dự kiến:

Plain Text
Boss Prefab = Practice_FinalBoss / PracticeEnemyHealth
Boss Spawn Point= BossSpawnPoint
Boss Container = RuntimeBosses
Active Boss = None ngoài Play Mode
Scene Hierarchy chính
Plain Text
Practice_CoreLoop
├── PracticeGame
│ ├── Systems
│ │ ├── PracticePlayerLevelSystem
│ │ ├── PracticeSurvivalTimer
│ │ ├── PracticeKillCounter
│ │ ├── PracticeRunScore
│ │ ├── PracticeBestScore
│ │ ├── PracticeGameStateController
│ │ └── PracticeRunDirector
│ │
│ └── Gameplay
│ ├── BossSystem
│ │ ├── FinalBossController
│ │ └── BossSpawnPoint
│ ├── Player
│ ├── Combat
│ ├── EnemySystem
│ ├── RuntimeEnemies
│ ├── RuntimeBosses
│ └── SpawnPoints
│
├── SceneLoaderRoot
└── PracticeAudioRoot

Ngoài Play Mode:

Không có Practice_FinalBoss đặt sẵn.
RuntimeBosses phải rỗng.
Boss được spawn tại runtime.
2.14. Boss Radial Attack
Trạng thái

Đã được xác nhận chạy xong, nhưng mức polish và hiểu biết kiến trúc từng được đánh giá là chưa hoàn toàn chắc chắn. Feature cần được giữ trong danh sách regression test cho đến khi branch được merge sạch và Console đạt chuẩn.

PracticeBossAttackDefinition

ScriptableObject giữ data của một attack.

Các field:

attackId.
displayName.
initialDelay.
telegraphDuration.
attackCooldown.
projectilePrefab.
projectileCount.
projectileSpeed.
projectileDamage.
projectileLifetime.
startingAngleOffset.

Trách nhiệm:

Mô tả attack bắn gì và thông số bao nhiêu.

Không có trách nhiệm:

Chạy coroutine.
Tìm Player.
Spawn projectile.
Gây damage trực tiếp.
Quản lý Game State.

Asset hiện có:

Plain Text
BossAttackDefinition_RadialBurst
PracticeBossAttackController

Trách nhiệm:

Resolve Boss Health.
Resolve Game State Controller.
Nghe Boss chết.
Chờ Initial Delay.
Bật Telegraph.
Chờ Telegraph Duration.
Tắt Telegraph.
Tính hướng vòng tròn.
Spawn projectile.
Chờ cooldown.
Lặp lại.
Dừng khi:
Boss chết.
Game Over.
Victory.
Component bị disable.
Tôn trọng Pause và Upgrade Choice.

Công thức radial:

Plain Text
angleStep = 360 / projectileCount
angle = startingAngleOffset + angleStep × index
direction = (cos(angle), sin(angle))

Với tám viên:

Plain Text
0°, 45°, 90°, 135°, 180°, 225°, 270°, 315°
PracticeBossProjectile

Trách nhiệm của một projectile:

Nhận direction.
Nhận speed.
Nhận damage.
Nhận lifetime.
Di chuyển bằng Rigidbody2D.MovePosition trong FixedUpdate.
Phát hiện Player.
Gọi PracticePlayerHealth.TakeDamage.
Chống damage lặp.
Biến mất khi trúng.
Biến mất khi hết lifetime.

Prefab hiện dùng:

Plain Text
Assets/Practice/Prefabs/Combat/Boss/BossProjectile_Test

Không được giữ đồng thời:

PracticeBulletMovement.
PracticeBulletDamageDealer.
PracticeBulletLifetime.

nếu PracticeBossProjectile đã chịu ba trách nhiệm đó.

FirePoint

Child của Boss prefab.

Trách nhiệm:

Chỉ làm marker vị trí spawn projectile.
Không cần script.
Không cần renderer.
Không cần collider.
AttackTelegraph

Child của Boss prefab.

Trách nhiệm:

Hiện cảnh báo trước khi Boss bắn.
Không gây damage.
Không có collider gameplay.
Chỉ là presentation/readability.

Flow:

Plain Text
Boss chuẩn bị attack
→ AttackTelegraph bật
→ chờ Telegraph Duration
→ AttackTelegraph tắt
→ projectile được spawn
Physics Layer

Đã định nghĩa:

Sorting Layer BossProjectile.
Physics Layer BossProjectile, User Layer 9.

Layer Matrix dự kiến:

Plain Text
BossProjectile ↔ PlayerBody Bật
BossProjectile ↔ Enemy Tắt
BossProjectile ↔ BossProjectile Tắt
BossProjectile ↔ PlayerSensor Tắt
BossProjectile ↔ Pickup Tắt
BossProjectile ↔ Default Có thể tắt
2.15. Boss Phase / Attack Selection
Trạng thái

Chưa được xác nhận hoàn thành.

Đã có thiết kế và code đề xuất nhằm nâng Boss từ một attack cố định thành:

Plain Text
Boss spawn
→ Phase 1
→ chọn attack
→ thực hiện attack
→ cooldown
→ attack kế tiếp
→ máu qua threshold
→ Phase 2
→ dùng attack data khó hơn
→ Boss chết
→ cleanup
→ Victory

Các thành phần đã được đề xuất nhưng chưa nên ghi nhận là production-complete:

PracticeBossPhaseDefinition.
Danh sách phase trong PracticeBossAttackController.
Phase threshold theo normalized health.
Cooldown multiplier.
Random selection.
Anti-repeat attack selection.
Phase 2 variant của Radial Burst.

Không được push/merge phần này cho đến khi:

Source thực tế compile.
Inspector được setup.
Phase 1/2 chuyển đúng.
Không có duplicate attack loop.
Boss death cleanup đúng.
Restart reset Phase 1.
Console 0 Error, 0 Warning.
3. Quy chuẩn Code & Kiến trúc
3.1. Single Responsibility

Mỗi class phải có một lý do chính để thay đổi.

Ví dụ đúng:

Plain Text
PracticePlayerExperience
→ nhận và phát XP
 
PracticePlayerLevelSystem
→ quản lý progression và Level Up
 
PracticeUpgradeChoiceUI
→ presentation và input của upgrade panel
 
PracticeShooterUpgradeApplier
→ áp dụng effect lên Shooter

Không gộp XP, Level, UI và Upgrade vào một PlayerSystem khổng lồ.

Việc tách Experience khỏi Level System đã được chốt sau khi phát hiện nhiều hệ thống upgrade phụ thuộc trực tiếp vào PracticePlayerLevelSystem.

3.2. Guard Clauses

Dùng return sớm để tránh nested if.

Ví dụ:

C#
if (isDead)
{
return;
}
 
if (damage <= 0f)
{
return;
}

Lợi ích:

Flow dễ đọc.
Edge case nằm ở đầu method.
Main behavior không bị lồng quá sâu.
Dễ debug.

Guard clause không được dùng để che lỗi cấu hình quan trọng. Dependency bắt buộc bị thiếu nên có log rõ ràng trong development thay vì lặng lẽ return.

3.3. Encapsulation

Dữ liệu runtime không được mở public field tùy tiện.

Ưu tiên:

C#
[SerializeField]
private float maxHealth;
 
public float MaxHealth => maxHealth;

Mục tiêu:

Inspector vẫn cấu hình được.
Script ngoài chỉ đọc nếu không được phép sửa.
Modification đi qua public API có validation.
3.4. Enum thay cho magic string

Đã dùng:

PracticeGameState.
PracticeRunPhase.

Không dùng:

C#
if (state == "Running")

Ưu tiên:

C#
if (state == PracticeGameState.Running)

Stable content ID vẫn dùng string cho dữ liệu content, nhưng phải được validate và không dùng tùy tiện cho runtime FSM.

3.5. Event-driven communication

Dùng C# Event/Action cho giao tiếp một-nhiều:

ExperienceGained.
ExperienceProgressChanged.
LeveledUp.
StateChanged.
damageTaken.
Died.

Flow tiêu chuẩn:

Plain Text
Owner thay đổi dữ liệu
→ owner phát event
→ listener cập nhật presentation hoặc subsystem

Ví dụ:

Plain Text
Health giảm
→ Health phát DamageTaken
→ Damage Feedback flash
→ Boss Health UI cập nhật
→ Phase System kiểm tra threshold

Không để Health biết trực tiếp Slider, TMP Text hay VFX object.

3.6. Subscribe/Unsubscribe đối xứng

Nếu subscribe trong OnEnable:

C#
source.Event += HandleEvent;

thì unsubscribe trong OnDisable:

C#
source.Event -= HandleEvent;

Yêu cầu:

Không để listener từ object cũ tồn tại sau Restart.
Không subscribe nhiều lần.
Có thể dùng defensive pattern:
C#
source.Event -= HandleEvent;
source.Event += HandleEvent;

nhưng vẫn phải unsubscribe đúng lifecycle.

3.7. FSM và centralized state

PracticeGameStateController là owner của gameplay state.

Không cho:

Pause UI tự đặt Time.timeScale.
Upgrade UI tự giữ game pause độc lập.
Game Over UI tự điều khiển tất cả subsystem.
Nhiều script cạnh tranh ghi time scale.

Flow chuẩn:

Plain Text
Subsystem/UI gửi request
→ State Controller validate transition
→ đổi state
→ cập nhật Time.timeScale
→ phát event
3.8. Phân biệt Game State và Run Phase

Không hợp nhất hai khái niệm:

Game State
Plain Text
Running
Paused
UpgradeChoice
GameOver
Victory
Run Phase
Plain Text
None
Survival
BossFight
Completed

Ví dụ:

Plain Text
Run Phase = BossFight
Game State = UpgradeChoice

Điều này có nghĩa run đang ở Boss Fight nhưng gameplay tạm dừng để chọn upgrade.

3.9. Data-driven bằng ScriptableObject

Đã áp dụng bước đầu cho:

Upgrade Definition.
Boss Attack Definition.

Mục tiêu cuối:

Weapon Definition.
Projectile Definition.
Enemy Definition.
Upgrade Definition V2.
Boss Definition.
Boss Phase Definition.
Loot Definition.
Spawn tables/catalogs.

Nguyên tắc:

ScriptableObject giữ design-time data.
Không giữ mutable runtime state.
Không sửa trực tiếp asset khi game đang chạy.
Runtime modifier phải tính thành local/runtime value.

Ví dụ đúng:

Plain Text
Base cooldown trong asset
× Phase multiplier
= Runtime cooldown

Không sửa attackCooldown của shared asset trong Phase 2.

3.10. Stable Content ID

Content cần ID ổn định:

Plain Text
boss_radial_burst
enemy_fast
weapon_default_shooter
upgrade_bullet_damage

ID dùng cho:

Catalog lookup.
Save/Load.
Validation.
Migration.
Debug.
Content unlock.

Không đồng nhất displayName với stable ID vì tên hiển thị có thể đổi hoặc được localization.

3.11. UI tách Game Logic

Quy tắc bắt buộc:

Game Logic không biết Button.
Game Logic không biết TMP_Text.
Game Logic không biết Panel.
UI nghe event.
UI gửi request.
UI không trực tiếp sửa gameplay private field.
Không tạo global event bus khổng lồ.

Ví dụ:

Plain Text
Restart Button
→ UI gửi request
→ Scene Loader xử lý restart

Không để Button trực tiếp gọi hàng loạt gameplay system.

3.12. Dependency management

Định hướng hiện tại:

Ưu tiên Inspector reference đối với dependency trong cùng prefab/scene.
Có auto-load cho component cùng GameObject.
Runtime prefab không thể tham chiếu trực tiếp scene object theo cách thông thường, nên có thể cần resolve hoặc injection.
Không lạm dụng FindFirstObjectByType trong hot path.
Không gọi Find mỗi frame.
Dependency bắt buộc phải được validate.

Về lâu dài:

Cached reference.
Runtime injection.
Factory/spawner khởi tạo dependency.
Catalog/service có phạm vi rõ ràng.
Tránh Service Locator toàn cục khó debug.
3.13. Lifecycle discipline

Phải xác định rõ:

Awake: khởi tạo state nội bộ.
OnEnable: subscribe và bắt đầu behavior có thể lặp.
Start: chỉ dùng nếu cần chờ các Awake hoàn tất.
Update: input/non-physics frame logic.
FixedUpdate: Rigidbody/physics movement.
OnDisable: unsubscribe, stop coroutine, cleanup visual.
OnDestroy: cleanup cuối cùng nếu có tài nguyên ngoài lifecycle enable/disable.

Đối tượng pooled sẽ không luôn bị Destroy, vì vậy cleanup không thể chỉ đặt trong OnDestroy.

3.14. Coroutine ownership và cleanup

Mỗi coroutine quan trọng phải có owner rõ ràng.

Ví dụ Boss Attack:

Controller giữ Coroutine.
Không start loop thứ hai nếu loop cũ đang chạy.
Boss chết:
Stop coroutine.
Tắt Telegraph.
Không spawn projectile mới.
Disable:
Stop coroutine.
Reset runtime flag.

Không dùng nhiều coroutine cạnh tranh thay đổi cùng một visual/state mà không có coordination.

3.15. Physics rules
Rigidbody movement dùng trong FixedUpdate.
Physics object không nên vừa đổi Transform trực tiếp vừa dùng Rigidbody.
Dùng Layer Matrix để giảm callback không cần thiết.
Phân biệt:
PlayerBody.
PlayerSensor.
Enemy.
Pickup.
BossProjectile.
Projectile dùng trigger khi chỉ cần phát hiện hit.
Continuous collision cho projectile nhanh khi cần.
Không dùng tag/layer thay thế lẫn nhau thiếu kiểm soát.
3.16. Duplicate responsibility prevention

Trước khi thêm component/script mới phải kiểm tra:

Đã có object cùng trách nhiệm chưa?
Projectile đã có movement chưa?
Đã có lifetime controller chưa?
Health owner nằm ở đâu?
UI owner nằm ở đâu?
Run Director có đang điều phối cùng flow không?

Ví dụ Boss Projectile không được giữ đồng thời:

Plain Text
PracticeBossProjectile
PracticeBulletMovement
PracticeBulletDamageDealer
PracticeBulletLifetime

nếu PracticeBossProjectile đã kiêm cả ba hành vi.

3.17. Object Pooling standards

Object Pooling là milestone bắt buộc nhưng chưa triển khai đầy đủ.

Đối tượng bắt buộc đánh giá/pool:

Player Bullet.
Boss Projectile.
Enemy.
XP Orb.
Damage Number.
Hit VFX.
Death VFX.

Mỗi object khi lấy từ pool phải reset:

Health.
Target.
Direction.
Velocity.
Damage.
Lifetime.
Collider.
Renderer.
Event subscription.
Coroutine.
Flash color.
hasDealtDamage.
Runtime flags.
Parent/container nếu cần.

Không chỉ thay:

C#
Destroy(gameObject);

bằng:

C#
gameObject.SetActive(false);

mà bỏ qua reset state.

3.18. Naming conventions

Định hướng cần chuẩn hóa:

Class/property/event: PascalCase.
Private field: camelCase.
Serialized private field: camelCase.
Method: PascalCase.
Boolean: is, has, can, should.
Stable ID: lowercase snake case.
Prefab/asset naming nhất quán.

Điểm cần refactor:

C#
public event Action<float> damageTaken;

nên cân nhắc:

C#
public event Action<float> DamageTaken;

Không nhất thiết đổi ngay nếu gây ảnh hưởng lớn, nhưng phải đưa vào technical debt list.

3.19. Comment standards

Comment nên giải thích:

Vì sao.
Constraint.
Quyết định kiến trúc.
Edge case.

Không cần comment lặp lại chính xác điều code đã nói.

Ví dụ tốt:

C#
// Chỉ phát event khi health thật sự giảm để feedback
// không chạy với damage bị chặn hoặc giá trị không hợp lệ.

Comment cũ/comment-out code không còn dùng nên xóa khỏi production sau khi Git đã lưu lịch sử.

Ví dụ trong PracticeEnemyHealth:

C#
//public event Action Died;

không cần giữ vì event đã được chuyển lên PracticeHealthBase.

3.20. Definition of Done

Một feature chỉ hoàn tất khi:

Chạy đúng.
Không duplicate responsibility.
Người phát triển kể lại được master flow.
Biết script nào giữ data.
Biết script nào bắt đầu flow.
Biết event/method chính.
Biết Hierarchy/Prefab/Inspector setup.
Biết tối thiểu ba checkpoint debug.
Biết cleanup xảy ra ở đâu.
Test restart/re-entry.
Console 0 Error.
Console 0 Warning.
Commit đúng branch.
PR vào production.
4. Lộ trình Các Tính năng CÒN THIẾU
Priority 0: Chốt trạng thái Git và baseline hiện tại

Trước khi bắt đầu feature mới:

Kiểm tra branch:
Shell
git branch --show-current
git status --short
Xác định Boss Radial Attack:
Đã commit chưa?
Đã push chưa?
Đã tạo PR chưa?
Đã merge vào production chưa?
Nếu chưa merge:
Chốt Radial Attack riêng.
Không gộp Boss Phase vào cùng commit nếu có thể tránh.
Sau merge:
Đồng bộ production.
Tạo branch mới.

Branch dự kiến cho feature tiếp theo:

Plain Text
feature/boss-phase-attack-selection
Priority 1: Hoàn thành P1 Complete Run Loop
4.1. Boss Radial Attack regression/polish

Dù đã chạy, vẫn cần khóa các test:

Telegraph bật trước khi bắn.
Telegraph tắt đúng thời điểm.
Pause giữa telegraph.
Upgrade Choice giữa telegraph.
Boss chết giữa telegraph.
Boss chết trong cooldown.
Victory không để attack mới sinh ra.
Restart không nhân đôi event/coroutine.
Projectile cũ không gây damage sau Victory nếu thiết kế yêu cầu cleanup.
Console sạch.
4.2. Boss Phase / Attack Selection

Trạng thái: đã thiết kế, chưa xác nhận hoàn thành.

Yêu cầu:

Phase 1.
Phase 2.
Threshold theo normalized health.
Attack list theo phase.
Chọn attack.
Không chạy hai attack cùng lúc.
Anti-repeat khi có nhiều attack.
Phase change event/presentation.
Reset Phase 1 khi Restart.
Không sửa ScriptableObject runtime.
4.3. Thêm Boss attack behavior thứ hai

Hiện executor vẫn chủ yếu là Radial Burst.

Cần tối thiểu một loại attack mới để chứng minh kiến trúc selection thực sự hoạt động, ví dụ:

Aimed Burst.
Spread Shot.
Spiral Burst.
Ring Burst luân phiên offset.
Area telegraph.
Summon Enemy, nếu phù hợp.

Không thêm bằng switch khổng lồ trong một controller.

Cần thiết kế:

Plain Text
Attack Definition
→ Attack Execution module/strategy
→ shared Boss combat coordinator
4.4. Boss phase transition presentation

Cần:

Visual flash hoặc color shift.
Animation/scale pulse.
SFX chuyển phase.
Có thể dừng ngắn hoặc tạo recovery window.
Không để phase transition gây attack overlap.
4.5. Boss result cleanup

Khi Boss chết/Victory:

Dừng attack.
Tắt telegraph.
Không nhận damage thêm.
Boss projectile:
Cleanup, hoặc
Disable damage và tự hết lifetime.
Dừng Boss BGM khi hệ thống audio hoàn thiện.
Kích hoạt Victory presentation đúng một lần.
Priority 2: Production UI Foundation
4.6. UI Style Guide

Cần xác định:

Color palette.
Primary/secondary/accent colors.
Health/XP/Boss colors.
Font hierarchy.
Heading/body/caption sizes.
Icon style.
Panel background style.
Border/radius/shadow.
4.7. Reusable components
Reusable button style.
Reusable panel style.
Reusable modal.
Reusable tooltip.
Reusable stat row.
Reusable slider style.
4.8. Responsive layout

Cần kiểm tra:

Canvas Scaler.
Reference Resolution.
Match Width/Height.
Anchors.
Safe Area nếu cần.
Các tỉ lệ:
16:9.
16:10.
21:9.
Cửa sổ nhỏ.
Không chồng HUD.
Result panel không tràn màn hình.
4.9. UI interaction states

Mỗi button cần:

Normal.
Highlighted.
Pressed.
Selected nếu cần.
Disabled.
Audio feedback.
Animation hook.
4.10. Panel animation
Pause.
Upgrade.
Game Over.
Victory.
Settings.
Boss warning.
Boss phase transition.

Game Logic chỉ phát event/state, UI tự animate.

Priority 3: Data-Driven Foundation
4.11. Stable IDs

Tạo chuẩn ID và validation:

Không để trống.
Không trùng.
Format thống nhất.
Không đổi ID tùy tiện sau khi save system sử dụng.
4.12. Definitions

Bắt buộc:

WeaponDefinition.
ProjectileDefinition.
EnemyDefinition.
UpgradeDefinitionV2.
BossDefinition.
BossPhaseDefinition.
LootDefinition.
4.13. Catalogs

Cần:

Weapon Catalog.
Enemy Catalog.
Upgrade Catalog.
Boss Catalog.
Loot Catalog.

Catalog hỗ trợ:

Lookup bằng stable ID.
Validate duplicate ID.
Content discovery.
Save migration.
4.14. Validation Tooling
Missing prefab.
Missing icon.
Negative values.
Invalid threshold.
Duplicate ID.
Empty attack list.
Circular/invalid dependency.
Catalog chứa null entry.
Priority 4: Upgrade Architecture V2
4.15. Nhóm upgrade
Offensive
Damage.
Fire Rate.
Projectile Count.
Critical Chance.
Critical Damage.
Pierce.
Projectile Speed.
Area.
Additional weapon.
Defensive
Max Health.
Armor.
Damage Reduction.
Regeneration.
Invulnerability duration.
Shield nếu phù hợp.
Utility
Movement Speed.
Pickup Radius.
XP multiplier.
Magnet.
Luck.
Reroll.
Drop chance.
4.16. Effect routing

Không dùng một switch khổng lồ:

C#
switch (upgradeType)
{
// hàng chục case
}

Cần:

Upgrade effect handlers.
Targeted receiver interfaces.
Data-driven parameters.
Runtime level tracking.
Validation xem receiver phù hợp có tồn tại không.
4.17. Runtime upgrade state
Upgrade ID.
Current level.
Max level.
Stacking rules.
Prerequisite/exclusion.
Reset mỗi run.
Có thể serialize meta upgrade riêng nếu cần.
Priority 5: Weapon Architecture
4.18. Weapon Definition

Dữ liệu:

ID.
Display name.
Icon.
Base damage.
Fire interval.
Projectile definition.
Firing pattern.
Targeting strategy.
Audio/VFX reference.
4.19. Weapon Runtime

Runtime state:

Current level.
Cooldown.
Modifier stack.
Target.
Projectile count.
Pattern state.
4.20. Targeting strategies
Nearest.
Lowest health.
Highest health.
Random.
Forward cone.
Boss priority.

Không hard-code tất cả vào Shooter.

4.21. Firing patterns
Single.
Spread.
Radial.
Burst.
Orbit.
Chain.
Area.
4.22. Projectile behaviors
Straight.
Homing.
Pierce.
Bounce.
Explode.
Split.
Return.
Priority 6: Enemy và Boss Content
Enemy archetypes
Grunt.
Fast.
Tank.
Ranged.
Exploder.
Elite.
Boss.

Mỗi archetype cần:

Data definition.
Runtime behavior.
Visual distinction.
Damage/health balance.
Spawn cost.
Drop table.
Audio/VFX.
Spawn tables
Theo thời gian.
Theo difficulty tier.
Theo spawn budget.
Giới hạn loại Enemy.
Elite schedule.
Boss exclusivity.
Priority 7: Pickup và Reward

Cần:

XP tiers.
Heal pickup.
Magnet pickup.
Currency.
Temporary buff nếu phù hợp.
Drop tables.
Drop chances.
Pickup pooling.
Pickup attraction budget.
Priority 8: Audio và VFX
Audio foundation cần hoàn thiện
Tạo và lưu PracticeAudioRoot prefab chuẩn.
Đặt persistent root đúng scene/bootstrap.
Tránh duplicate Audio Manager.
Test Mixer parameter.
Test volume conversion tuyến tính sang decibel.
Lưu settings.
BGM
Main Menu.
Gameplay.
Boss Fight.
Victory.
Game Over nếu cần.
Crossfade hoặc transition.
SFX
Player shoot.
Bullet hit.
Enemy hit.
Enemy death.
XP pickup.
Level up.
Upgrade select.
Reroll.
Skip.
Boss warning.
Boss telegraph.
Boss fire.
Boss phase change.
Boss death.
Game Over.
Victory.
Button hover/click.
VFX
Muzzle flash.
Hit impact.
Enemy death.
XP pickup.
Level Up.
Upgrade select.
Player damage.
Player death.
Boss spawn.
Boss warning.
Boss telegraph.
Boss phase transition.
Victory.
Budget
Audio voice budget.
VFX instance budget.
Priority/culling.
Pool frequent effects.
Priority 9: JSON Save và Meta Progression
SaveData

Cần chứa:

Save version.
Currency.
Unlocks.
Permanent upgrades.
Statistics.
Settings nếu phù hợp.
Last write timestamp nếu cần.
Yêu cầu bắt buộc
JSON serialization.
Không save mỗi frame.
Atomic save hoặc quy trình giảm nguy cơ hỏng file.
Backup.
Corrupted save recovery.
Version migration.
Stable ID.
Default save.
Validate giá trị load.
Không serialize reference Unity trực tiếp.
Statistics
Total runs.
Wins.
Deaths.
Best time.
Best score.
Total kills.
Boss kills.
Highest level.
Play time nếu có.
Priority 10: Object Pooling

Ứng viên theo ưu tiên:

Player Bullet.
Boss Projectile.
XP Orb.
Enemy.
Hit VFX.
Death VFX.
Damage Number.

Pool contract nên có:

Plain Text
OnTakenFromPool
Initialize/Configure
OnReturnedToPool
ResetRuntimeState

Cần test:

Reuse nhiều lần.
Không giữ event cũ.
Không giữ target cũ.
Không giữ velocity.
Không flash sai màu.
Không damage ngay khi được lấy lại.
Lifetime reset.
Collider reset.
Priority 11: Performance Architecture
Profiler

Theo dõi:

CPU.
Rendering.
Physics 2D.
GC Alloc.
Memory.
Audio.
Timeline.
Tối ưu bắt buộc
Tránh allocation mỗi frame.
Tránh LINQ trong hot path.
Cache reference.
Không Find mỗi frame.
Object Pooling.
Physics Layer Matrix.
Target Registry.
Spawn budget.
VFX budget.
Audio voice budget.
Hạn chế coroutine số lượng lớn nếu gây overhead.
Không tạo material instance ngoài ý muốn.
Target Registry

Shooter không nên liên tục scan toàn scene.

Cần registry:

Plain Text
Enemy spawn/enable
→ đăng ký
 
Enemy death/disable
→ hủy đăng ký
 
Shooter
→ query registry
→ chọn target
Priority 12: Benchmark và Stress Test

Tạo Benchmark Scene riêng.

Các mốc:

100 entity.
250 entity.
500 entity.
750 entity.
Khoảng 1.000 entity.

Đo:

FPS.
Main thread time.
Physics time.
GC alloc/frame.
Active projectile.
Active Enemy.
Pool size.
Memory.
Draw calls/batches nếu có.
Soak test
Chạy dài.
Theo dõi memory growth.
Theo dõi object leak.
Theo dõi event subscription leak.
Theo dõi pool growth không giới hạn.
Theo dõi performance degradation theo thời gian.
Báo cáo before/after
Baseline.
Bottleneck.
Thay đổi.
Kết quả.
Trade-off.
Screenshot Profiler.
Build/hardware test.
Priority 13: Polish và Release
Art Player.
Art Enemy.
Art Boss.
Animation.
UI polish.
Audio đầy đủ.
VFX đầy đủ.
Balance.
QA checklist.
Resolution test.
Input test.
Windows build.
Itch.io page.
README.
Screenshots.
Trailer.
Architecture documentation.
Performance report.
Known issues.
Credits/licenses.
5. Vấn đề / Bug cần lưu ý
5.1. Boss Phase chưa phải feature hoàn thành

Mặc dù đã có thiết kế/code đề xuất, chưa có xác nhận:

Compile thành công.
Asset được tạo.
Prefab được setup.
Phase chuyển đúng.
Restart reset đúng.
Git branch riêng.
PR đã merge.

Không được ghi Boss Phase vào release notes như một feature đã hoàn thành.

5.2. Boss Attack Controller có nguy cơ phình to

Nếu tiếp tục thêm:

Radial.
Aimed.
Spiral.
Area.
Summon.
Dash.
Phase selection.
Cooldown.
Telegraph.
Animation.
Audio.

vào cùng PracticeBossAttackController, class sẽ thành God Object.

Hướng refactor khi thêm attack behavior thứ hai:

Plain Text
BossCombatCoordinator
├── Phase Resolver
├── Attack Selector
└── Attack Executor modules
├── Radial Attack
├── Aimed Attack
└── Area Attack

Không cần tách quá sớm, nhưng phải tách ngay khi behavior thứ hai chứng minh abstraction cần thiết.

5.3. Attack Definition đang có thể phụ thuộc behavior radial

Nếu PracticeBossAttackDefinition chứa trực tiếp:

Projectile Count.
Starting Angle Offset.
Radial-specific field.

thì nó chưa phải definition tổng quát cho mọi Boss attack.

Giải pháp tương lai:

Base attack definition + behavior-specific definition.
Hoặc composition bằng execution strategy/module.
Không thêm hàng loạt field nullable dành cho mọi loại attack vào một asset.
5.4. Initial Delay semantics cần làm rõ

Nếu Boss Phase selector chọn attack ở mỗi vòng, InitialDelay không nên áp dụng lại cho mọi attack trừ khi có chủ ý.

Cần phân biệt:

Boss initial combat delay.
Telegraph delay.
Attack-specific wind-up.
Cooldown/recovery.

Nếu không, chuyển phase hoặc chọn attack mới có thể bị delay không mong muốn.

5.5. Projectile cleanup khi Victory

Cần quyết định chính thức:

Phương án A

Projectile đã bắn tiếp tục tồn tại đến hết lifetime nhưng không gây damage sau Victory.

Phương án B

Victory cleanup toàn bộ Boss Projectile ngay lập tức.

Đối với presentation sạch, phương án B thường rõ hơn. Tuy nhiên cleanup nên đi qua registry/container/pool, không dùng scene-wide search thường xuyên.

5.6. Destroy(gameObject) chưa tương thích Object Pooling

PracticeEnemyHealth.Die() đang Destroy Enemy.

Khi triển khai pooling:

Health không nên tự quyết định Destroy.
Health nên phát Died.
Lifecycle/pool owner quyết định return object.
Hoặc Enemy có death handler riêng.

Nếu giữ Destroy trong base flow, pooling sẽ khó thay thế sạch.

5.7. Health event naming

Hiện có:

C#
public event Action<float> damageTaken;

Theo convention nên là:

C#
public event Action<float> DamageTaken;

Đây là technical debt nhỏ nhưng nên xử lý trước khi số listener tăng lớn.

5.8. Health API chưa đủ cho production

Còn thiếu:

Heal.
Change Max Health.
Restore Full Health.
Normalized Health.
HealthChanged event.
Reset for pool.
Optional invulnerability contract.
Damage context/source.
Damage type nếu gameplay cần.

Không thêm tất cả cùng lúc. Bổ sung theo nhu cầu thực tế của Defensive Upgrade và Pooling.

5.9. Damage event chỉ truyền float

damageTaken(float) đủ cho prototype, nhưng production có thể cần biết:

Damage source.
World hit point.
Critical hay không.
Damage type.
Instigator.
Knockback.
Hit direction.

Khi thực sự cần, nên tạo DamageContext hoặc DamageInfo, nhưng tránh over-engineer trước khi có ít nhất hai hệ thống sử dụng dữ liệu đó.

5.10. ScriptableObject runtime mutation

Rủi ro:

Sửa attack count/cooldown trực tiếp trên asset khi chuyển phase.
Thay đổi có thể ảnh hưởng tất cả instance dùng chung.
Có thể giữ giá trị bất ngờ trong Editor tùy cách thao tác.
Restart khó reset.

Quy tắc:

Plain Text
Definition là readonly design data
Runtime state nằm trong runtime object
Modifier tạo calculated value
5.11. FindFirstObjectByType là dependency fallback

Có thể chấp nhận khi Boss prefab runtime cần tìm Game State Controller một lần.

Không được:

Gọi mỗi frame.
Gọi mỗi attack.
Dùng cho mọi subsystem.
Che việc dependency bị thiếu.

Trong production, nên cân nhắc Spawner inject context vào Boss sau khi instantiate.

5.12. Upgrade UI state bug cần regression test

Bug cũ đã sửa:

Plain Text
Upgrade panel đóng
nhưng state vẫn UpgradeChoice
→ Time.timeScale = 0

Phải test lại sau mọi refactor Game State/UI:

Chọn upgrade.
Skip.
Reroll rồi chọn.
Nhiều Level Up cùng lúc.
Player chết gần thời điểm Level Up.
Boss Fight Level Up.
Restart khi modal vừa đóng.
5.13. Pending multiple Level Ups

PracticePlayerLevelSystem có thể xử lý nhiều Level Up trong một lần nhận XP.

Upgrade UI cần đảm bảo:

Plain Text
Level up nhiều lần
→ pending choices tăng
→ người chơi giải quyết từng lựa chọn
→ chỉ quay lại Running khi pending = 0

Không đóng UI sau lựa chọn đầu nếu còn pending choice.

5.14. Event leak sau Restart

Các object persistent hoặc static event là điểm nguy hiểm.

Cần kiểm tra:

Scene Loader.
Audio Manager.
Settings.
Game State listener.
Boss UI listener.
Level System listener.
Upgrade UI listener.

Triệu chứng:

Callback chạy hai lần.
Boss bắn hai vòng.
Score tăng hai lần.
UI update object đã destroy.
MissingReferenceException.
5.15. Persistent root duplicate

SceneLoaderRoot và PracticeAudioRoot nếu dùng DontDestroyOnLoad phải chống duplicate.

Flow nguy hiểm:

Plain Text
Main Menu có AudioRoot
→ Gameplay cũng đặt AudioRoot
→ chuyển scene
→ tồn tại hai manager
→ nhạc/SFX chạy hai lần

Cần bootstrap strategy hoặc singleton ownership có lifecycle rõ ràng.

5.16. Audio chưa hoàn tất

Không được đánh dấu Audio System complete chỉ vì đã có:

Mixer.
Sources.
Settings fields.

Còn thiếu:

Clip library.
Playback policy.
Music transition.
SFX routing.
Voice limit.
Prefab.
Scene persistence.
Full runtime test.
Save settings.
5.17. UI mới ở mức foundation

Victory Result UI đã có responsive desktop cơ bản nhưng toàn bộ UI production còn thiếu:

Art direction thống nhất.
Nhiều aspect ratio.
Controller/keyboard navigation nếu cần.
Selection state.
Animation.
Audio.
Safe area.
Localization readiness.
5.18. Targeting có nguy cơ tốn hiệu năng

Nếu Shooter dùng:

FindObjectsByType.
Physics overlap lớn mỗi frame.
Duyệt toàn bộ Enemy mỗi frame.

thì sẽ trở thành bottleneck khi stress test.

Cần:

Target Registry.
Query interval.
Spatial filtering khi cần.
Cache nearest target hợp lý.
Re-evaluate khi target chết/out-of-range.
5.19. Destroy/Instantiate pressure

Các object tần suất cao hiện có nguy cơ dùng Instantiate/Destroy:

Player Bullet.
Boss Projectile.
Enemy.
XP Orb.
VFX.

Điều này sẽ gây:

GC pressure.
Main thread spikes.
Memory churn.
Stutter trong wave lớn.

Object Pooling là milestone bắt buộc, không phải tùy chọn.

5.20. Telegraph presentation chưa production-ready

Telegraph hiện là placeholder SpriteRenderer.

Cần về sau:

Kích thước gắn với vùng nguy hiểm thực.
Màu đủ tương phản.
Animation charge.
Timing khớp chính xác với fire.
Không bị Boss sprite che.
Không nhầm với pickup hoặc player effect.
SFX anticipation.
Accessibility/readability.

Telegraph không được chỉ đẹp mà sai hit timing.

5.21. Physics Matrix cần audit sau khi thêm layer

Mỗi layer mới có thể làm tăng số cặp collision.

Cần duy trì tài liệu:

Plain Text
Layer
→ tương tác với layer nào
→ trigger/collision
→ lý do

Boss Projectile chỉ nên tương tác với Player Body và những lớp thật sự cần.

5.22. Comment-out code cần dọn

Ví dụ trong PracticeEnemyHealth còn comment cũ về Died.

Git đã lưu lịch sử nên không cần giữ code chết/comment-out lâu dài.

Dọn sau khi feature ổn định:

Comment cũ.
Tester script không còn dùng.
Debug key.
Legacy field.
Duplicate component.
Asset thử nghiệm không còn reference.
5.23. Tester scripts không vào production build nếu không cần

Các script như upgrade tester bằng phím số hữu ích khi học, nhưng production cần:

Đưa vào Development-only tooling.
Tắt khỏi prefab release.
Hoặc xóa sau khi có debug console/test scene.
Không để người chơi kích hoạt cheat ngoài ý muốn.
5.24. Chưa có automated validation/test

Hiện phần lớn kiểm thử thủ công.

Nên bổ sung theo mức phù hợp:

EditMode tests cho calculation.
PlayMode tests cho state transition.
Asset validation menu.
Catalog ID validation.
Boss phase threshold tests.
Save migration tests.
Pool reset tests.

Không cần test mọi MonoBehaviour ngay lập tức. Ưu tiên logic thuần và regression có rủi ro cao.

Kết luận chuyển giao

Dự án đã vượt qua giai đoạn học từng mechanic rời rạc và hiện có một gameplay loop tương đối hoàn chỉnh:

Plain Text
Main Menu
→ Loading
→ Survival
→ Auto-shoot Combat
→ XP/Level/Upgrade
→ Difficulty progression
→ Player Death hoặc Boss Fight
→ Final Boss
→ Victory
→ Result
→ Restart/Main Menu

Nền tảng mạnh hiện có:

Game State tập trung.
Run Phase riêng biệt.
Experience/Level được tách trách nhiệm.
Event-driven UI.
Scene transition.
Result flow.
Final Boss trigger.
Boss presentation.
Boss Radial Attack data-driven bước đầu.
Git workflow theo feature branch.

Những phần quan trọng nhất cần tiếp tục ngay:

Xác nhận và chốt Git cho Boss Radial Attack.
Hoàn thành Boss Phase / Attack Selection trên branch riêng.
Thêm ít nhất một Boss attack behavior thứ hai.
Hoàn thành toàn bộ P1 Complete Run Loop.
Xây Production UI Foundation.
Chuyển content sang data-driven có stable ID và catalog.
Upgrade Architecture V2.
Weapon Architecture.
Enemy/Boss content.
Audio/VFX hoàn chỉnh.
JSON persistence.
Object Pooling.
Profiling, benchmark và stress test.
Polish, QA và release.

Nguyên tắc phát triển tiếp theo không thay đổi:

Không cần viết tay lại toàn bộ code đã hiểu, nhưng phải hiểu owner, dependency, master flow, event, lifecycle, cleanup, setup Unity và checkpoint debug. Một feature không được xem là hoàn tất chỉ vì “đã nhìn thấy nó chạy”, mà phải chịu được pause, state transition, death, restart, cleanup và regression test với Console sạch.
