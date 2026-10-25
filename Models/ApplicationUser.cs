using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace QuanLyThuVIen.Models
{
    // Tài khoản người dùng, kế thừa IdentityUser để dùng luôn cơ chế đăng nhập/đăng ký/băm mật khẩu
    // và đăng nhập bằng Google/Facebook (external login) có sẵn của ASP.NET Core Identity.
    // Các trường thêm bên dưới là thông tin cá nhân hiển thị ở trang "Trang cá nhân".
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        // URL ảnh đại diện (không lưu file nhị phân, chỉ lưu link) - để trống sẽ hiển thị icon mặc định
        [StringLength(500, ErrorMessage = "Đường dẫn ảnh tối đa 500 ký tự")]
        [Display(Name = "Ảnh đại diện (URL)")]
        public string? AvatarUrl { get; set; }

        // Ngày tạo tài khoản, dùng để hiển thị trên trang cá nhân
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
