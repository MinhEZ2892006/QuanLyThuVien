namespace QuanLyThuVIen.Models
{
    // Tình trạng của một đầu sách (bản sao vật lý) cụ thể
    public enum BookCopyStatus
    {
        Available,  // Còn
        Borrowed,   // Đang cho mượn
        Lost,       // Mất
        Damaged     // Hỏng
    }
}
