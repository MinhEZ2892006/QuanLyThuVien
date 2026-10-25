namespace QuanLyThuVIen.Models
{
    // Tình trạng của 1 dòng mượn (1 đầu sách trong phiếu mượn)
    public enum LoanDetailStatus
    {
        Borrowed,   // Đang mượn
        Returned,   // Đã trả
        Lost,       // Báo mất
        Damaged     // Báo hỏng
    }
}
