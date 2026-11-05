public interface IPracticePoolable//để hàm nào cũng có thể sử dụng được
{
    void OnTakenFromPool();//Đưa toàn bộ các biến đếm thời gian, cờ va chạm, hướng di chuyển và vận tốc vật lý về trạng thái mặc định ban đầu.
    void OnReturnedToPool();//Chạy ngay khi đối tượng được lấy từ hàng đợi Pool ra và bật SetActive(true) (dùng để tính lại mốc thời gian sống destroyTime hoặc bật lại Collider2D).
    void ResetRuntimeState();//Chạy ngay trước khi đối tượng bị tắt SetActive(false) để cất vào kho (dùng để tắt Collider2D, dừng Coroutine, xóa vận tốc vật lý).
}