using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVIen.Models;

namespace QuanLyThuVIen.Controllers
{
    public class BookController : Controller
    {
        private readonly LibraryDbContext _context;

        public BookController(LibraryDbContext context)
        {
            _context = context;
        }

        // Số sách hiển thị tối đa trên mỗi trang danh sách.
        private const int PageSize = 5;

        // Danh sách toàn bộ mã sách, có thể lọc theo từ khóa tìm kiếm (Mã sách/Tên sách/Tác giả/
        // Thể loại/Nhà xuất bản), và phân trang (mỗi trang tối đa PageSize cuốn) để tránh tải
        // một lèo toàn bộ dữ liệu khi số lượng sách lớn. Gọi ToList() ngay để tránh xung đột
        // DataReader khi View duyệt dữ liệu.
        public IActionResult Index(string? searchString, int page = 1)
        {
            ViewData["CurrentFilter"] = searchString;

            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var keyword = searchString.Trim();
                query = query.Where(b =>
                    b.BookCode.Contains(keyword) ||
                    b.Title.Contains(keyword) ||
                    b.Author.Contains(keyword) ||
                    (b.Genre != null && b.Genre.Contains(keyword)) ||
                    (b.Publisher != null && b.Publisher.Contains(keyword)));
            }

            query = query.OrderBy(b => b.BookCode);

            var totalCount = query.Count();
            var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)PageSize);

            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var books = query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            return View(books);
        }

        // Chi tiết 1 mã sách kèm danh sách đầu sách (BookCopy) để View/partial hiển thị (dùng ở bước 2.3).
        // Include BookCopies ngay trong Controller, đúng quy tắc không truy vấn DbContext trong View.
        public IActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var book = _context.Books
                .Include(b => b.BookCopies)
                .FirstOrDefault(b => b.BookCode == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // Hiển thị form thêm mã sách mới
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý thêm mã sách mới. Kiểm tra trùng BookCode trước khi lưu vì BookCode là khóa
        // chính tự nhiên do thủ thư tự nhập, không phải khóa tự tăng nên rất dễ trùng.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Book book)
        {
            if (_context.Books.Any(b => b.BookCode == book.BookCode))
            {
                ModelState.AddModelError(nameof(Book.BookCode), "Mã sách đã tồn tại");
            }

            if (!ModelState.IsValid)
            {
                return View(book);
            }

            _context.Books.Add(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // Hiển thị form sửa mã sách theo BookCode
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var book = _context.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // Xử lý cập nhật mã sách. Không cho đổi BookCode (khóa chính) qua form Edit;
        // chỉ cập nhật từng field trên entity đang được EF theo dõi (không gán đè cả object)
        // để tránh vô tình ghi đè navigation property BookCopies.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Book book)
        {
            if (id != book.BookCode)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(book);
            }

            var existing = _context.Books.Find(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Title = book.Title;
            existing.Author = book.Author;
            existing.Genre = book.Genre;
            existing.PublicationYear = book.PublicationYear;
            existing.Publisher = book.Publisher;
            existing.ImageUrl = book.ImageUrl;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // Hiển thị trang xác nhận xóa, kèm số lượng đầu sách hiện có để cảnh báo trước
        [HttpGet]
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var book = _context.Books
                .Include(b => b.BookCopies)
                .FirstOrDefault(b => b.BookCode == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // Xử lý xóa mã sách. Chặn xóa nếu còn BookCopy liên quan (dù DbContext đã cấu hình
        // Cascade cho quan hệ này, vẫn chặn ở tầng nghiệp vụ để thủ thư không xóa nhầm mất
        // dữ liệu lịch sử của các đầu sách đang tồn tại).
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var book = _context.Books
                .Include(b => b.BookCopies)
                .FirstOrDefault(b => b.BookCode == id);

            if (book == null)
            {
                return NotFound();
            }

            if (book.BookCopies.Any())
            {
                ModelState.AddModelError(string.Empty, "Không thể xóa mã sách này vì còn đầu sách (BookCopy) liên quan");
                return View("Delete", book);
            }

            _context.Books.Remove(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
