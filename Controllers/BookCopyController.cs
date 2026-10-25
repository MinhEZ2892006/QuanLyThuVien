using Microsoft.AspNetCore.Mvc;
using QuanLyThuVIen.Models;

namespace QuanLyThuVIen.Controllers
{
    public class BookCopyController : Controller
    {
        private readonly LibraryDbContext _context;

        public BookCopyController(LibraryDbContext context)
        {
            _context = context;
        }

        // Hiển thị form thêm đầu sách (BookCopy) mới cho 1 mã sách.
        // bookCode được truyền qua query string từ nút "+ Thêm đầu sách" ở trang Details của Book.
        [HttpGet]
        public IActionResult Create(string bookCode)
        {
            if (string.IsNullOrEmpty(bookCode))
            {
                return NotFound();
            }

            var book = _context.Books.Find(bookCode);
            if (book == null)
            {
                return NotFound();
            }

            // Gán sẵn BookCode vào model để form không cần người dùng tự nhập lại mã sách
            var bookCopy = new BookCopy { BookCode = bookCode };
            ViewBag.BookTitle = book.Title;

            return View(bookCopy);
        }

        // Xử lý thêm đầu sách mới. Kiểm tra trùng (BookCode, ControlNumber) ở tầng Controller
        // trước khi lưu, dù DB đã có unique index (cấu hình ở LibraryDbContext), để trả lỗi
        // rõ ràng lên form thay vì để văng DbUpdateException.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookCopy bookCopy)
        {
            var book = _context.Books.Find(bookCopy.BookCode);
            if (book == null)
            {
                return NotFound();
            }

            if (_context.BookCopies.Any(c => c.BookCode == bookCopy.BookCode && c.ControlNumber == bookCopy.ControlNumber))
            {
                ModelState.AddModelError(nameof(BookCopy.ControlNumber), "Số kiểm soát này đã tồn tại cho mã sách này");
            }

            // Đầu sách mới thêm luôn ở tình trạng "Còn"; không cho chọn tình trạng khác ngay lúc tạo
            // (đổi tình trạng thủ công là chức năng riêng ở bước 2.2)
            bookCopy.Status = BookCopyStatus.Available;
            ModelState.Remove(nameof(BookCopy.Status));

            if (!ModelState.IsValid)
            {
                ViewBag.BookTitle = book.Title;
                return View(bookCopy);
            }

            _context.BookCopies.Add(bookCopy);
            _context.SaveChanges();

            return RedirectToAction("Details", "Book", new { id = bookCopy.BookCode });
        }

        // Hiển thị form đổi tình trạng thủ công (Hỏng/Mất) cho 1 đầu sách
        [HttpGet]
        public IActionResult ChangeStatus(int id)
        {
            var bookCopy = _context.BookCopies.Find(id);
            if (bookCopy == null)
            {
                return NotFound();
            }

            return View(bookCopy);
        }

        // Xử lý đổi tình trạng thủ công. Chỉ cho đổi khi đầu sách đang "Còn"; nếu đang "Cho mượn"
        // thì phải xử lý qua chức năng Báo mất/Báo hỏng khi trả sách (giai đoạn 5) để đồng bộ với
        // LoanDetail/CompensationCase, không cho sửa tắt trực tiếp ở đây.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeStatus(int id, BookCopyStatus newStatus)
        {
            var bookCopy = _context.BookCopies.Find(id);
            if (bookCopy == null)
            {
                return NotFound();
            }

            if (bookCopy.Status == BookCopyStatus.Borrowed)
            {
                ModelState.AddModelError(string.Empty, "Đầu sách đang cho mượn, phải xử lý qua chức năng Báo mất/Báo hỏng khi trả sách");
                return View(bookCopy);
            }

            if (newStatus != BookCopyStatus.Lost && newStatus != BookCopyStatus.Damaged)
            {
                ModelState.AddModelError(string.Empty, "Chỉ được đổi thủ công sang tình trạng Mất hoặc Hỏng");
                return View(bookCopy);
            }

            bookCopy.Status = newStatus;
            _context.SaveChanges();

            return RedirectToAction("Details", "Book", new { id = bookCopy.BookCode });
        }
    }
}
