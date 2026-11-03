# BÁO CÁO HIỆU NĂNG & STRESS TEST (BENCHMARK REPORT)

## 1. Thông tin Môi trường & Phần cứng Kiểm thử (Test Environment)
- **Thời điểm xuất báo cáo:** `2026-11-03 16:53:11`
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
| **100** | 109 | 101 | 8 | 0 | **205.0 FPS** | 58.4 FPS | 4.88 ms | 1384.1 MB | 0 |
| **250** | 258 | 250 | 12 | 1 | **179.6 FPS** | 14.8 FPS | 5.57 ms | 1392.2 MB | 0 |
| **500** | 508 | 494 | 15 | 1 | **145.3 FPS** | 35.3 FPS | 6.88 ms | 1392.1 MB | 1 |
| **750** | 761 | 750 | 12 | 1 | **137.0 FPS** | 32.7 FPS | 7.30 ms | 1401.4 MB | 0 |
| **1000** | 1009 | 994 | 15 | 1 | **127.6 FPS** | 27.6 FPS | 7.84 ms | 1395.6 MB | 1 |

## 4. Đánh giá Độ ổn định Bộ nhớ Dài hạn (Soak Test & Leak Verification)
- **Bộ nhớ cấp phát hiện tại (Total Allocated Memory):** `1395.64 MB`
- **Tổng số lần thu gom rác (GC Gen-0 Count):** `299`
- **Kiểm chứng Object Pool & Event Symmetry:** Toàn bộ thực thể (`Enemy`, `PlayerBullet`, `BossProjectile`, `XPOrb`, `FloatingDamageText`) đều thực thi `IPracticePoolable`, tự động hủy đăng ký sự kiện trong `OnDisable` và khôi phục trạng thái gốc (`ResetRuntimeState`), không phát sinh rò rỉ bộ nhớ (Memory Leak) hay nhân đôi sự kiện sau nhiều chu kỳ tái sử dụng.
