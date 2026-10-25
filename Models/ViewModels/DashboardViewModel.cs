namespace QuanLyThuVIen.Models.ViewModels
{
    // Số liệu tổng hợp hiển thị trên trang chủ/Dashboard - bước 7.3.
    // Không map trực tiếp CSDL, chỉ gom số liệu để View hiển thị.
    public class DashboardViewModel
    {
        // Tổng số mã sách (Book)
        public int TotalBooks { get; set; }

        // Tổng số đầu sách (BookCopy) thuộc tất cả mã sách
        public int TotalBookCopies { get; set; }

        // Số đầu sách đang cho mượn (BookCopyStatus.Borrowed)
        public int BorrowedCopies { get; set; }

        // Số đầu sách đang bị mất hoặc hỏng (BookCopyStatus.Lost/Damaged)
        public int LostOrDamagedCopies { get; set; }

        // Tổng số thẻ thư viện
        public int TotalCards { get; set; }

        // Số thẻ đang bị khóa (LibraryCardStatus.Inactive)
        public int LockedCards { get; set; }

        // Số phiếu mượn đang hoạt động (còn ít nhất 1 LoanDetail ở trạng thái Borrowed)
        public int ActiveLoans { get; set; }

        // Số case bồi thường chưa xử lý (IsResolved = false)
        public int UnresolvedCompensationCases { get; set; }
    }
}
