using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVIen.Models
{
    // Thẻ thư viện do sinh viên đăng ký để mượn sách
    public class LibraryCard
    {
        // Số thẻ là khóa chính tự nhiên, do thư viện cấp (ví dụ: TV0001)
        [Key]
        [Required(ErrorMessage = "Số thẻ không được để trống")]
        [StringLength(20, ErrorMessage = "Số thẻ tối đa 20 ký tự")]
        [Display(Name = "Số thẻ")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên sinh viên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ tên sinh viên")]
        public string StudentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày hết hạn không được để trống")]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        [Display(Name = "Ngày hết hạn")]
        public DateTime ExpiryDate { get; set; }

        // Tình trạng sử dụng: Active (sử dụng được) / Inactive (không sử dụng)
        // Bị chuyển Inactive khi: hết hạn, hoặc mất/hỏng sách chưa bồi thường xong
        [Display(Name = "Tình trạng")]
        public LibraryCardStatus Status { get; set; } = LibraryCardStatus.Active;
    }
}
