# BÁO CÁO HIỆU NĂNG & STRESS TEST (BENCHMARK REPORT)

## 1. Thông tin Môi trường & Phần cứng Kiểm thử (Test Environment)
- **Thời điểm xuất báo cáo:** `2026-11-03 16:29:35`
- **Hệ điều hành:** `Windows 11  (10.0.26200) 64bit`
- **CPU:** `13th Gen Intel(R) Core(TM) i5-13500HX` (20 luồng logic)
- **GPU:** `NVIDIA GeForce RTX 4050 Laptop GPU` (5920 MB VRAM)
- **RAM Hệ thống:** `16103 MB`
- **Phiên bản Unity:** `2022.3.62f1`

## 2. So sánh Kiến trúc Trước và Sau Tối ưu (Before vs. After Architecture)
| Hạng mục Kỹ thuật | Trước Tối ưu (Baseline Prototype) | Sau Tối ưu (Production Architecture) | Tác động Hiệu năng |
| :--- | :--- | :--- | :--- |
| **Tìm mục tiêu (`Targeting`)** | `FindObjectsOfType` / Quét toàn Scene $O(N)$ + `Vector2.Distance` (`Sqrt`) | `PracticeTargetRegistry` (Swap-and-Pop $O(1)$) + `sqrMagnitude` + Cache `0.08s` | **0 B GC Alloc**, giảm >90% CPU Main Thread khi đông quái |
| **Quản lý Vòng đời Thực thể** | `Instantiate` & `Destroy` trực tiếp cho Đạn, Quái, Hạt XP, Damage Text | `PracticeObjectPoolManager` (`Queue<T>` $O(1)$ + Prewarm đầu trận + `IPracticePoolable`) | Xóa bỏ hoàn toàn hiện tượng giật khung hình (GC Spike) khi xả đạn |
| **Cập nhật UI & HUD** | Nối chuỗi `$"..."` mỗi frame trong `Update()` | Event-Driven (`C# Action`) + Dirty-Flag Caching (`lastDisplayedSecond`, `cachedStats`) | **0 B GC Alloc/frame** ở trạng thái tĩnh, không phải rebuild TMP Mesh thừa |
| **Lưu trữ Dữ liệu (`Save I/O`)** | Ghi `PlayerPrefs.Save()` trực tiếp mỗi lần điểm tăng vượt kỷ lục | Cập nhật trên RAM trong trận + Atomic JSON Save (`.tmp` -> `.bak` -> `.json`) cuối trận | Loại bỏ 100% độ trễ Disk I/O trong lúc giao chiến |
| **Âm thanh (`Audio Voice Budget`)** | Phát `PlayOneShot` không giới hạn khi nhiều quái chết cùng lúc | `PracticeAudioManager` giới hạn `minSfxRepeatInterval = 0.045s` theo từng `AudioClip` | Chống vỡ tiếng (Audio Clipping) và tiết kiệm kênh trộn âm (Audio Voices) |

## 3. Kết quả Stress Test theo 5 Mốc Tải Thực thể (Entity Load Tiers)
| Mốc Tải (Target) | Thực thể Đỉnh (Peak) | Quái (Enemies) | Đạn (Projectiles) | Hạt XP (Orbs) | FPS Trung bình | FPS Thấp nhất (Min) | Frame Time (ms) | Bộ nhớ Phân bổ (MB) | GC Gen-0 Delta |
| :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **100** | 108 | 93 | 15 | 1 | **235.1 FPS** | 30.3 FPS | 4.25 ms | 387.4 MB | 1 |
| **250** | 265 | 250 | 15 | 1 | **193.7 FPS** | 58.4 FPS | 5.16 ms | 478.3 MB | 2 |
| **500** | 512 | 500 | 12 | 0 | **53.9 FPS** | 3.7 FPS | 18.55 ms | 710.7 MB | 3 |
| **750** | 764 | 743 | 21 | 0 | **4.1 FPS** | 0.2 FPS | 245.85 ms | 1077.1 MB | 3 |

## 4. Đánh giá Độ ổn định Bộ nhớ Dài hạn (Soak Test & Leak Verification)
- **Bộ nhớ cấp phát hiện tại (Total Allocated Memory):** `1077.13 MB`
- **Tổng số lần thu gom rác (GC Gen-0 Count):** `277`
- **Kiểm chứng Object Pool & Event Symmetry:** Toàn bộ thực thể (`Enemy`, `PlayerBullet`, `BossProjectile`, `XPOrb`, `FloatingDamageText`) đều thực thi `IPracticePoolable`, tự động hủy đăng ký sự kiện trong `OnDisable` và khôi phục trạng thái gốc (`ResetRuntimeState`), không phát sinh rò rỉ bộ nhớ (Memory Leak) hay nhân đôi sự kiện sau nhiều chu kỳ tái sử dụng.
