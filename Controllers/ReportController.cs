using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVIen.Models;

namespace QuanLyThuVIen.Controllers
{
    public class ReportController : Controller
    {
        private readonly LibraryDbContext _context;

        public ReportController(LibraryDbContext context)
        {
            _context = context;
        }

        // Báo cáo sách mất/hỏng theo năm, tính theo năm phát sinh case bồi thường (CreatedDate)
        // vì đây là ngày thủ thư ghi nhận mất/hỏng, đúng với nghiệp vụ "báo cáo hàng năm để thanh lý".
        // year = null: hiển thị toàn bộ các năm, không lọc.
        [HttpGet]
        public IActionResult Index(int? year, string? searchString)
        {
            // Lấy trước danh sách các năm đã từng phát sinh case (chưa lọc) để đổ vào dropdown lọc trên View
            ViewBag.Years = _context.CompensationCases
                .Select(c => c.CreatedDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            var query = _context.CompensationCases
                .Include(c => c.LoanDetail)
                    .ThenInclude(ld => ld!.BookCopy)
                        .ThenInclude(bc => bc!.Book)
                .Include(c => c.LibraryCard)
                .AsQueryable();

            if (year.HasValue)
            {
                query = query.Where(c => c.CreatedDate.Year == year.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var keyword = searchString.Trim();
                query = query.Where(c =>
                    c.CardNumber.Contains(keyword) ||
                    (c.LibraryCard != null && c.LibraryCard.StudentName.Contains(keyword)) ||
                    (c.LoanDetail != null && c.LoanDetail.BookCopy != null && c.LoanDetail.BookCopy.BookCode.Contains(keyword)) ||
                    (c.LoanDetail != null && c.LoanDetail.BookCopy != null && c.LoanDetail.BookCopy.Book != null && c.LoanDetail.BookCopy.Book.Title.Contains(keyword)));
            }

            ViewBag.SelectedYear = year;
            ViewData["CurrentFilter"] = searchString;

            var cases = query
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            return View(cases);
        }
    }
}
