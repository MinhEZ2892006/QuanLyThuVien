using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVIen.Models
{
    // Case bồi thường: phát sinh khi 1 dòng mượn (LoanDetail) bị báo mất/hỏng.
    // Thẻ liên quan sẽ bị khóa (Inactive) cho đến khi TOÀN BỘ case chưa xử lý của thẻ
    // được đánh dấu đã bồi thường (IsResolved = true) thì mới mở lại (CompensationService xử lý).
    public class CompensationCase
    {
        [Key]
        public int CompensationCaseId { get; set; }

        // Dòng mượn (đầu sách cụ thể) bị mất/hỏng làm phát sinh case này
        [Required]
        public int LoanDetailId { get; set; }

        [ForeignKey(nameof(LoanDetailId))]
        public LoanDetail? LoanDetail { get; set; }

        // Lưu thêm CardNumber (thay vì chỉ đi qua LoanDetail -> Loan) để truy vấn
        // "danh sách case chưa xử lý của 1 thẻ" được nhanh, không phải join nhiều bảng
        [Required(ErrorMessage = "Số thẻ không được để trống")]
        [StringLength(20)]
        [Display(Name = "Số thẻ")]
        public string CardNumber { get; set; } = string.Empty;

        [ForeignKey(nameof(CardNumber))]
        public LibraryCard? LibraryCard { get; set; }

        [Display(Name = "Lý do")]
        public CompensationReason Reason { get; set; }

        [Required(ErrorMessage = "Ngày phát sinh không được để trống")]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        [Display(Name = "Ngày phát sinh")]
        public DateTime CreatedDate { get; set; }

        // Đã bồi thường xong hay chưa
        [Display(Name = "Đã bồi thường")]
        public bool IsResolved { get; set; } = false;

        // Ngày bồi thường xong; null nếu case chưa xử lý
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        [Display(Name = "Ngày bồi thường")]
        public DateTime? ResolvedDate { get; set; }
    }
}
