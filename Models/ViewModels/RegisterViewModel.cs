using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVIen.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa {1} ký tự", MinimumLength = 2)]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        // [EmailAddress] bắt buộc chuỗi phải có dạng email hợp lệ (bao gồm ký tự @)
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ (phải có dạng ten@miền.com)")]
        [StringLength(256, ErrorMessage = "Email tối đa {1} ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        // Mật khẩu: tối thiểu 6 ký tự, tối đa 100 ký tự, và bắt buộc có cả chữ lẫn số
        // (RegularExpression kiểm tra ngay ở Client lẫn Server; Identity PasswordOptions ở
        // Program.cs cũng ép cùng quy tắc này ở tầng lưu tài khoản để chắc chắn không bị bỏ qua).
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải từ {2} đến {1} ký tự", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Mật khẩu phải có cả chữ và số")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu")]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
