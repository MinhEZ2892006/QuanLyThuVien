using Microsoft.AspNetCore.Mvc;
using QuanLyThuVIen.Models;
using QuanLyThuVIen.Services;

namespace QuanLyThuVIen.Controllers
{
    public class CompensationController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly CompensationService _compensationService;

        public CompensationController(LibraryDbContext context, CompensationService compensationService)
        {
            _context = context;
            _compensationService = compensationService;
        }

        // Trang Bồi thường & mở lại thẻ: danh sách case bồi thường chưa xử lý của 1 thẻ - bước 6.3
        [HttpGet]
        public IActionResult Index(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
            {
                return NotFound();
            }

            var card = _context.LibraryCards.Find(cardNumber);
            if (card == null)
            {
                return NotFound();
            }

            var cases = _compensationService.GetUnresolvedCases(cardNumber);
            ViewBag.Card = card;

            return View(cases);
        }

        // Xử lý đánh dấu 1 case đã bồi thường xong. Ngày bồi thường lấy theo ngày hiện tại
        // của hệ thống (thủ thư xác nhận ngay lúc thao tác), cùng cách làm với Return/ReportLostOrDamaged.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Resolve(int id, string cardNumber)
        {
            var result = _compensationService.Resolve(id, DateTime.Today);

            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Index), new { cardNumber });
        }
    }
}
