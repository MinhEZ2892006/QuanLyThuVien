using Microsoft.AspNetCore.Mvc;
using QuanLyThuVIen.Models;

namespace QuanLyThuVIen.Controllers
{
    public class LibraryCardController : Controller
    {
        private readonly LibraryDbContext _context;

        public LibraryCardController(LibraryDbContext context)
        {
            _context = context;
        }

        // Danh sách toàn bộ thẻ thư viện, có thể lọc theo từ khóa (Số thẻ/Họ tên sinh viên).
        // Gọi ToList() ngay để tránh xung đột DataReader khi View duyệt.
        public IActionResult Index(string? searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var query = _context.LibraryCards.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var keyword = searchString.Trim();
                query = query.Where(c =>
                    c.CardNumber.Contains(keyword) ||
                    c.StudentName.Contains(keyword));
            }

            var cards = query
                .OrderBy(c => c.CardNumber)
                .ToList();

            return View(cards);
        }

        // Chi tiết 1 thẻ thư viện. Danh sách phiếu mượn đang hoạt động (bước 4.6) và trang
        // bồi thường/mở lại thẻ (bước 6.3) chỉ đặt link sang LoanController/CompensationController
        // ở View, không load sẵn dữ liệu ở đây để tránh Controller này phải biết logic của Service khác.
        public IActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var card = _context.LibraryCards.Find(id);
            if (card == null)
            {
                return NotFound();
            }

            return View(card);
        }

        // Hiển thị form cấp thẻ thư viện mới
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý cấp thẻ mới. Kiểm tra trùng CardNumber trước khi lưu vì CardNumber là khóa
        // chính tự nhiên do thư viện tự đặt, không phải khóa tự tăng nên rất dễ trùng.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LibraryCard card)
        {
            if (_context.LibraryCards.Any(c => c.CardNumber == card.CardNumber))
            {
                ModelState.AddModelError(nameof(LibraryCard.CardNumber), "Số thẻ đã tồn tại");
            }

            // Thẻ mới cấp luôn ở tình trạng "Sử dụng được"; không cho chọn tình trạng khác lúc cấp thẻ
            card.Status = LibraryCardStatus.Active;
            ModelState.Remove(nameof(LibraryCard.Status));

            if (!ModelState.IsValid)
            {
                return View(card);
            }

            _context.LibraryCards.Add(card);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // Hiển thị form gia hạn thẻ theo CardNumber
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var card = _context.LibraryCards.Find(id);
            if (card == null)
            {
                return NotFound();
            }

            return View(card);
        }

        // Xử lý gia hạn thẻ: CHỈ cập nhật Ngày hết hạn, không đụng tới Họ tên/Tình trạng.
        // Đổi tình trạng (khóa/mở) là chức năng riêng, xem action ToggleStatus bên dưới.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, LibraryCard card)
        {
            if (id != card.CardNumber)
            {
                return NotFound();
            }

            var existing = _context.LibraryCards.Find(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (card.ExpiryDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(LibraryCard.ExpiryDate), "Ngày hết hạn mới phải từ hôm nay trở đi");
            }

            if (!ModelState.IsValid)
            {
                // Giữ lại Họ tên/Tình trạng thật của thẻ để hiển thị đúng trên form khi có lỗi
                card.StudentName = existing.StudentName;
                card.Status = existing.Status;
                return View(card);
            }

            existing.ExpiryDate = card.ExpiryDate;
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = existing.CardNumber });
        }

        // Hiển thị trang xác nhận khóa/mở thẻ thủ công
        [HttpGet]
        public IActionResult ToggleStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var card = _context.LibraryCards.Find(id);
            if (card == null)
            {
                return NotFound();
            }

            return View(card);
        }

        // Xử lý khóa/mở thẻ thủ công.
        // - Khóa tay (Active -> Inactive): luôn cho phép.
        // - Mở tay (Inactive -> Active): CHẶN nếu thẻ còn CompensationCase chưa xử lý (IsResolved = false),
        //   để nhất quán với CompensationService.TryReactivateCard (giai đoạn 6) - bắt buộc phải bồi thường
        //   xong thì thẻ mới được mở lại, không cho thủ thư override bỏ qua bước bồi thường.
        [HttpPost, ActionName("ToggleStatus")]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatusConfirmed(string id)
        {
            var card = _context.LibraryCards.Find(id);
            if (card == null)
            {
                return NotFound();
            }

            if (card.Status == LibraryCardStatus.Inactive)
            {
                var hasUnresolvedCase = _context.CompensationCases
                    .Any(c => c.CardNumber == card.CardNumber && !c.IsResolved);

                if (hasUnresolvedCase)
                {
                    ModelState.AddModelError(string.Empty, "Không thể mở khóa thủ công: thẻ còn case bồi thường chưa xử lý. Phải bồi thường xong (Giai đoạn 6) để thẻ tự mở lại.");
                    return View("ToggleStatus", card);
                }
            }

            card.Status = card.Status == LibraryCardStatus.Active
                ? LibraryCardStatus.Inactive
                : LibraryCardStatus.Active;

            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = card.CardNumber });
        }
    }
}
