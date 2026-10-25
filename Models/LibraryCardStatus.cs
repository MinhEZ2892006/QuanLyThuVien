namespace QuanLyThuVIen.Models
{
    // Tình trạng sử dụng của thẻ thư viện
    public enum LibraryCardStatus
    {
        Active,     // Sử dụng được
        Inactive    // Không sử dụng (bị khóa do mất/hỏng sách chưa bồi thường, hoặc hết hạn)
    }
}
