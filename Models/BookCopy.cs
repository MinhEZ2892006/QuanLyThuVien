using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVIen.Models
{
    // Đầu sách (bản sao vật lý) của 1 mã sách, phân biệt bằng số kiểm soát
    // Ví dụ: mã sách "CT101" có thể có nhiều BookCopy với ControlNumber 001, 002, 003...
    public class BookCopy
    {
        [Key]
        public int BookCopyId { get; set; }

        [Required(ErrorMessage = "Mã sách không được để trống")]
        [StringLength(20)]
        [Display(Name = "Mã sách")]
        public string BookCode { get; set; } = string.Empty;

        // Navigation tới Book cha; FK thật sự (BookCode) được khai báo bằng chính property này
        [ForeignKey(nameof(BookCode))]
        public Book? Book { get; set; }

        // Số kiểm soát dùng để phân biệt các đầu sách cùng 1 mã sách (ví dụ: 001, 002...)
        // Lưu ý: cặp (BookCode, ControlNumber) sẽ được cấu hình unique index ở bước viết DbContext
        [Required(ErrorMessage = "Số kiểm soát không được để trống")]
        [StringLength(10, ErrorMessage = "Số kiểm soát tối đa 10 ký tự")]
        [Display(Name = "Số kiểm soát")]
        public string ControlNumber { get; set; } = string.Empty;

        [Display(Name = "Tình trạng")]
        public BookCopyStatus Status { get; set; } = BookCopyStatus.Available;

        [StringLength(250, ErrorMessage = "Ghi chú tối đa 250 ký tự")]
        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }
    }
}
