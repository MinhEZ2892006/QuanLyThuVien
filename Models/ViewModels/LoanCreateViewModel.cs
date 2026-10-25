using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVIen.Models.ViewModels
{
    // ViewModel cho form tạo phiếu mượn: người dùng chọn 1 thẻ thư viện và tối đa
    // 3 mã sách. Không map trực tiếp xuống bảng CSDL nào - Loan/LoanDetail thật sự
    // do LoanService.CreateLoan tạo ra từ dữ liệu của ViewModel này.
    public class LoanCreateViewModel
    {
        [Required(ErrorMessage = "Phải chọn thẻ thư viện")]
        [Display(Name = "Số thẻ")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày mượn không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày mượn")]
        public DateTime LoanDate { get; set; } = DateTime.Today;

        // 3 ô chọn mã sách riêng biệt (không dùng multi-select để tránh phải thêm
        // thư viện/JS plugin ngoài phạm vi cho phép). Ô nào không chọn để trống;
        // Controller sẽ gom lại thành danh sách và bỏ qua ô trống trước khi gọi LoanService.
        [Display(Name = "Mã sách 1")]
        public string? BookCode1 { get; set; }

        [Display(Name = "Mã sách 2")]
        public string? BookCode2 { get; set; }

        [Display(Name = "Mã sách 3")]
        public string? BookCode3 { get; set; }
    }
}
