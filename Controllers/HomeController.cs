using Microsoft.AspNetCore.Mvc;
using QuanLyThuVIen.Models;
using QuanLyThuVIen.Models.ViewModels;
using System.Diagnostics;

namespace QuanLyThuVIen.Controllers
{
    public class HomeController : Controller
    {
        private readonly LibraryDbContext _context;

        public HomeController(LibraryDbContext context)
        {
            _context = context;
        }

        // Trang chủ/Dashboard: số liệu tổng hợp toàn hệ thống - bước 7.3.
        // Mỗi số liệu chỉ là 1 câu lệnh Count đơn giản trên DbContext, không cần Include
        // vì View chỉ hiển thị con số, không hiển thị chi tiết từng bản ghi.
        public IActionResult Index()
        {
            var model = new DashboardViewModel
            {
                TotalBooks = _context.Books.Count(),
                TotalBookCopies = _context.BookCopies.Count(),
                BorrowedCopies = _context.BookCopies.Count(bc => bc.Status == BookCopyStatus.Borrowed),
                LostOrDamagedCopies = _context.BookCopies.Count(bc =>
                    bc.Status == BookCopyStatus.Lost || bc.Status == BookCopyStatus.Damaged),
                TotalCards = _context.LibraryCards.Count(),
                LockedCards = _context.LibraryCards.Count(c => c.Status == LibraryCardStatus.Inactive),
                ActiveLoans = _context.Loans.Count(l => l.LoanDetails.Any(ld => ld.Status == LoanDetailStatus.Borrowed)),
                UnresolvedCompensationCases = _context.CompensationCases.Count(c => !c.IsResolved)
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
