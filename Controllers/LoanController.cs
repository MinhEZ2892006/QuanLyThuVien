using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVIen.Models;
using QuanLyThuVIen.Models.ViewModels;
using QuanLyThuVIen.Services;

namespace QuanLyThuVIen.Controllers
{
    public class LoanController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly LoanService _loanService;

        public LoanController(LibraryDbContext context, LoanService loanService)
        {
            _context = context;
            _loanService = loanService;
        }

        // Tra cứu toàn bộ phiếu mượn: tìm theo số thẻ, họ tên sinh viên, mã phiếu mượn (LoanId)
        // hoặc tên/mã sách đang có trong phiếu. Đây là trang danh sách chính của mục "Mượn sách".
        [HttpGet]
        public IActionResult Index(string? searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var query = _context.Loans
                .Include(l => l.LibraryCard)
                .Include(l => l.LoanDetails)
                    .ThenInclude(ld => ld.BookCopy)
                        .ThenInclude(bc => bc!.Book)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var keyword = searchString.Trim();
                var isNumeric = int.TryParse(keyword, out var loanId);

                query = query.Where(l =>
                    l.CardNumber.Contains(keyword) ||
                    (l.LibraryCard != null && l.LibraryCard.StudentName.Contains(keyword)) ||
                    (isNumeric && l.LoanId == loanId) ||
                    l.LoanDetails.Any(ld =>
                        (ld.BookCopy != null && ld.BookCopy.BookCode.Contains(keyword)) ||
                        (ld.BookCopy != null && ld.BookCopy.Book != null && ld.BookCopy.Book.Title.Contains(keyword))));
            }

            var loans = query
                .OrderByDescending(l => l.LoanDate)
                .ThenByDescending(l => l.LoanId)
                .ToList();

            return View(loans);
        }

        // Hiển thị form tạo phiếu mượn mới
        [HttpGet]
        public IActionResult Create()
        {
            LoadSelectLists();

            var model = new LoanCreateViewModel { LoanDate = DateTime.Today };
            return View(model);
        }

        // Xử lý tạo phiếu mượn: gom 3 ô chọn mã sách thành danh sách rồi giao toàn bộ
        // việc validate + tạo Loan/LoanDetail cho LoanService.CreateLoan (đã viết ở 4.1-4.2),
        // Controller không tự kiểm tra lại nghiệp vụ để tránh trùng logic 2 nơi.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LoanCreateViewModel model)
        {
            var bookCodes = new[] { model.BookCode1, model.BookCode2, model.BookCode3 }
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Select(code => code!)
                .ToList();

            // Chỉ kiểm tra định dạng cơ bản (có chọn mã sách hay không) ở Controller;
            // các quy tắc nghiệp vụ còn lại (trùng mã, quá 3 mã, thẻ khóa/hết hạn...)
            // đều do LoanService.CreateLoan kiểm tra.
            if (bookCodes.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Phải chọn ít nhất 1 mã sách để mượn");
            }

            if (!ModelState.IsValid)
            {
                LoadSelectLists();
                return View(model);
            }

            var result = _loanService.CreateLoan(model.CardNumber, bookCodes, model.LoanDate);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                LoadSelectLists();
                return View(model);
            }

            // Sau khi mượn thành công, quay về trang danh sách phiếu mượn đang hoạt động
            // của chính thẻ vừa mượn để thủ thư thấy ngay kết quả (bước 4.6)
            return RedirectToAction(nameof(ActiveLoans), new { cardNumber = model.CardNumber });
        }

        // Danh sách phiếu mượn đang hoạt động (tính runtime) của 1 thẻ - bước 4.6
        [HttpGet]
        public IActionResult ActiveLoans(string cardNumber)
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

            var loans = _loanService.GetActiveLoans(cardNumber);
            ViewBag.Card = card;

            return View(loans);
        }

        // Hiển thị form xác nhận trả sách cho 1 dòng mượn - bước 5.3
        [HttpGet]
        public IActionResult Return(int id)
        {
            var detail = LoadLoanDetailForForm(id);
            if (detail == null)
            {
                return NotFound();
            }

            return View(detail);
        }

        // Xử lý trả sách. Ngày trả lấy theo ngày hiện tại của hệ thống (thủ thư xác nhận trả
        // ngay lúc thao tác), không cho tự nhập để tránh sai lệch dữ liệu.
        [HttpPost, ActionName("Return")]
        [ValidateAntiForgeryToken]
        public IActionResult ReturnConfirmed(int id)
        {
            var detail = _context.LoanDetails
                .Include(ld => ld.Loan)
                .FirstOrDefault(ld => ld.LoanDetailId == id);

            if (detail == null)
            {
                return NotFound();
            }

            var cardNumber = detail.Loan!.CardNumber;

            var result = _loanService.Return(id, DateTime.Today);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                var full = LoadLoanDetailForForm(id);
                if (full == null)
                {
                    return NotFound();
                }
                return View("Return", full);
            }

            return RedirectToAction(nameof(ActiveLoans), new { cardNumber });
        }

        // Hiển thị form báo mất/báo hỏng cho 1 dòng mượn - bước 5.4
        [HttpGet]
        public IActionResult ReportLostOrDamaged(int id)
        {
            var detail = LoadLoanDetailForForm(id);
            if (detail == null)
            {
                return NotFound();
            }

            return View(detail);
        }

        // Xử lý báo mất/báo hỏng. Ngày phát sinh lấy theo ngày hiện tại của hệ thống, cùng lý do
        // với action Return để đảm bảo dữ liệu báo cáo (Giai đoạn 7) chính xác theo thời gian thực.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReportLostOrDamaged(int id, CompensationReason reason)
        {
            var detail = _context.LoanDetails
                .Include(ld => ld.Loan)
                .FirstOrDefault(ld => ld.LoanDetailId == id);

            if (detail == null)
            {
                return NotFound();
            }

            var cardNumber = detail.Loan!.CardNumber;

            var result = _loanService.ReportLostOrDamaged(id, reason, DateTime.Today);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                var full = LoadLoanDetailForForm(id);
                if (full == null)
                {
                    return NotFound();
                }
                return View(full);
            }

            return RedirectToAction(nameof(ActiveLoans), new { cardNumber });
        }

        // Lấy 1 LoanDetail kèm BookCopy/Book/Loan để đổ đủ thông tin cho form Return/ReportLostOrDamaged,
        // dùng chung cho cả action GET và action POST khi cần hiển thị lại form lúc có lỗi.
        private LoanDetail? LoadLoanDetailForForm(int id)
        {
            return _context.LoanDetails
                .Include(ld => ld.BookCopy)
                    .ThenInclude(bc => bc!.Book)
                .Include(ld => ld.Loan)
                .FirstOrDefault(ld => ld.LoanDetailId == id);
        }

        // Chuẩn bị danh sách thẻ + danh sách sách để đổ vào 2 dropdown của form Create,
        // lấy dữ liệu phụ trợ 1 lần trong Controller rồi truyền qua ViewBag theo đúng quy tắc.
        private void LoadSelectLists()
        {
            ViewBag.Cards = _context.LibraryCards
                .OrderBy(c => c.CardNumber)
                .Select(c => new SelectListItem
                {
                    Value = c.CardNumber,
                    Text = c.CardNumber + " - " + c.StudentName
                })
                .ToList();

            var bookOptions = _context.Books
                .OrderBy(b => b.BookCode)
                .Select(b => new SelectListItem
                {
                    Value = b.BookCode,
                    Text = b.BookCode + " - " + b.Title
                })
                .ToList();

            ViewBag.Books = bookOptions;
        }
    }
}
