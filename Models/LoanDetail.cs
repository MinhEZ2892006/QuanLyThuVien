using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVIen.Models
{
    // 1 dòng trong phiếu mượn, ứng với 1 đầu sách (BookCopy) cụ thể đã được chọn khi mượn
    public class LoanDetail
    {
        [Key]
        public int LoanDetailId { get; set; }

        [Required]
        public int LoanId { get; set; }

        [ForeignKey(nameof(LoanId))]
        public Loan? Loan { get; set; }

        // Đầu sách cụ thể được chọn cho mã sách này khi mượn (chọn ở LoanService)
        [Required]
        public int BookCopyId { get; set; }

        [ForeignKey(nameof(BookCopyId))]
        public BookCopy? BookCopy { get; set; }

        // Ngày trả thực tế; null nếu chưa trả (đang mượn/đã báo mất/báo hỏng)
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        [Display(Name = "Ngày trả")]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Tình trạng")]
        public LoanDetailStatus Status { get; set; } = LoanDetailStatus.Borrowed;
    }
}
