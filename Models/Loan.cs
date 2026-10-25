using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVIen.Models
{
    // Phiếu mượn: 1 thẻ thư viện mượn tối đa 3 mã sách/lần, mỗi mã sách là 1 LoanDetail
    // Việc phiếu mượn còn "đang hoạt động" hay không được TÍNH RUNTIME dựa vào các
    // LoanDetail (còn dòng nào Status = Borrowed thì phiếu vẫn coi là đang hoạt động),
    // không lưu cột Status riêng ở đây để tránh dữ liệu bị lệch nhau.
    public class Loan
    {
        [Key]
        public int LoanId { get; set; }

        [Required(ErrorMessage = "Số thẻ không được để trống")]
        [StringLength(20)]
        [Display(Name = "Số thẻ")]
        public string CardNumber { get; set; } = string.Empty;

        [ForeignKey(nameof(CardNumber))]
        public LibraryCard? LibraryCard { get; set; }

        [Required(ErrorMessage = "Ngày mượn không được để trống")]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        [Display(Name = "Ngày mượn")]
        public DateTime LoanDate { get; set; }

        // Hạn trả: LoanDate + tối đa 15 ngày, việc kiểm tra 15 ngày do LoanService xử lý
        // Cột kiểu "date" (không có giờ/phút/giây) để so sánh hạn trả cho chính xác
        [Required(ErrorMessage = "Hạn trả không được để trống")]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        [Display(Name = "Hạn trả")]
        public DateTime DueDate { get; set; }

        // Danh sách mã sách được mượn trong phiếu này (tối đa 3 dòng)
        public ICollection<LoanDetail> LoanDetails { get; set; } = new List<LoanDetail>();
    }
}
