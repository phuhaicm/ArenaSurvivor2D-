using System.Collections.Generic;
using UnityEngine;

public class PracticeTargetRegistry : HaiMonoBehaviour//sử dụng biến static bởi vì Để PracticeNearestEnemyFinder (vốn là một static class) và PracticeEnemyHealth có thể truy cập trực tiếp ngay lập tức mà không cần tốn chi phí tìm kiếm component.
{
    public static PracticeTargetRegistry Instance { get; private set; }

    [Header("Runtime Debug (Read-Only)")]
    [SerializeField]
    private int activeTargetCount;

    // Cấp phát sẵn dung lượng 256 phần tử để List không phải cấp phát lại mảng khi quái đông
    private static readonly List<PracticeEnemyHealth> activeTargets =
        new List<PracticeEnemyHealth>(256);

    // Bảng tra cứu vị trí index của từng Enemy trong List để xóa với độ phức tạp O(1)
    private static readonly Dictionary<PracticeEnemyHealth, int> targetIndices =
        new Dictionary<PracticeEnemyHealth, int>(256);

    public static IReadOnlyList<PracticeEnemyHealth> ActiveTargets => activeTargets;
    public static int Count => activeTargets.Count;

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
        SyncDebugCount();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            ClearAll();
            Instance = null;
        }
    }

    public static void RegisterTarget(PracticeEnemyHealth target)
    {
        if (target == null || target.IsDead)
        {
            return;
        }

        // Guard Clause: Nếu quái đã có tên trong sổ đăng ký thì bỏ qua ngay (O(1))
        if (targetIndices.ContainsKey(target))
        {
            return;
        }

        int newIndex = activeTargets.Count;
        activeTargets.Add(target);
        targetIndices[target] = newIndex;

        SyncDebugCount();
    }

    public static void UnregisterTarget(PracticeEnemyHealth target)
    {
        if (target == null)
        {
            return;
        }

        if (!targetIndices.TryGetValue(target, out int removeIndex))
        {
            return;
        }

        int lastIndex = activeTargets.Count - 1;
        PracticeEnemyHealth lastTarget = activeTargets[lastIndex];

        // Kỹ thuật Swap-and-Pop O(1):
        // Đưa phần tử cuối danh sách lên lấp vào chỗ vừa bị xóa để không phải dịch chuyển mảng
        activeTargets[removeIndex] = lastTarget;
        targetIndices[lastTarget] = removeIndex;

        activeTargets.RemoveAt(lastIndex);
        targetIndices.Remove(target);

        SyncDebugCount();
    }

    public static void ClearAll()
    {
        activeTargets.Clear();
        targetIndices.Clear();
        SyncDebugCount();
    }

    private static void SyncDebugCount()
    {
        if (Instance != null)
        {
            Instance.activeTargetCount = activeTargets.Count;
        }
    }
}