using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVIen.Models
{
    // Một mã sách (Book) có thể ứng với nhiều đầu sách (BookCopy) khác nhau
    public class Book
    {
        // Mã sách là khóa chính tự nhiên, do thủ thư đặt (ví dụ: CT101)
        [Key]
        [Required(ErrorMessage = "Mã sách không được để trống")]
        [StringLength(20, ErrorMessage = "Mã sách tối đa 20 ký tự")]
        [Display(Name = "Mã sách")]
        public string BookCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên sách không được để trống")]
        [StringLength(200, ErrorMessage = "Tên sách tối đa 200 ký tự")]
        [Display(Name = "Tên sách")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tác giả không được để trống")]
        [StringLength(100, ErrorMessage = "Tên tác giả tối đa 100 ký tự")]
        [Display(Name = "Tác giả")]
        public string Author { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Thể loại tối đa 50 ký tự")]
        [Display(Name = "Thể loại")]
        public string? Genre { get; set; }

        // Giới hạn năm hợp lý để tránh nhập sai (không dùng năm âm hoặc quá xa thực tế)
        [Range(1000, 2100, ErrorMessage = "Năm xuất bản phải trong khoảng từ 1000 đến 2100")]
        [Display(Name = "Năm xuất bản")]
        public int PublicationYear { get; set; }

        [StringLength(150, ErrorMessage = "Tên nhà xuất bản tối đa 150 ký tự")]
        [Display(Name = "Nhà xuất bản")]
        public string? Publisher { get; set; }

        // Đường dẫn URL tới ảnh bìa sách (không lưu file nhị phân trong CSDL, chỉ lưu link).
        // Cho phép để trống; View sẽ hiển thị ảnh mặc định (icon sách) nếu không có ảnh.
        [StringLength(500, ErrorMessage = "Đường dẫn ảnh tối đa 500 ký tự")]
        [Display(Name = "Ảnh bìa (URL)")]
        [Url(ErrorMessage = "Đường dẫn ảnh không hợp lệ")]
        public string? ImageUrl { get; set; }

        // Danh sách đầu sách (bản sao vật lý) thuộc mã sách này
        // Khởi tạo sẵn danh sách rỗng để tránh NullReferenceException khi View duyệt
        // mà Controller quên .Include()
        public ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
    }
}
