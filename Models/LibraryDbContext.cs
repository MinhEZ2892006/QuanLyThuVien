using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace QuanLyThuVIen.Models
{
    // DbContext quản lý toàn bộ CSDL cho hệ thống quản lý thư viện.
    // Kế thừa IdentityDbContext<ApplicationUser> để có thêm các bảng tài khoản/đăng nhập
    // (AspNetUsers, AspNetUserLogins cho đăng nhập Google/Facebook, ...) từ ASP.NET Core Identity.
    public class LibraryDbContext : IdentityDbContext<ApplicationUser>
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books => Set<Book>();
        public DbSet<BookCopy> BookCopies => Set<BookCopy>();
        public DbSet<LibraryCard> LibraryCards => Set<LibraryCard>();
        public DbSet<Loan> Loans => Set<Loan>();
        public DbSet<LoanDetail> LoanDetails => Set<LoanDetail>();
        public DbSet<CompensationCase> CompensationCases => Set<CompensationCase>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // BookCopy: 1 mã sách (BookCode) không được trùng Số kiểm soát (ControlNumber)
            modelBuilder.Entity<BookCopy>()
                .HasIndex(bc => new { bc.BookCode, bc.ControlNumber })
                .IsUnique();

            // Book (1) - BookCopy (nhiều): xóa 1 mã sách thì xóa luôn các đầu sách của nó
            modelBuilder.Entity<BookCopy>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BookCopies)
                .HasForeignKey(bc => bc.BookCode)
                .OnDelete(DeleteBehavior.Cascade);

            // LibraryCard (1) - Loan (nhiều)
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.LibraryCard)
                .WithMany()
                .HasForeignKey(l => l.CardNumber)
                .OnDelete(DeleteBehavior.Cascade);

            // Loan (1) - LoanDetail (nhiều): xóa phiếu mượn thì xóa luôn các dòng mượn của nó
            modelBuilder.Entity<LoanDetail>()
                .HasOne(ld => ld.Loan)
                .WithMany(l => l.LoanDetails)
                .HasForeignKey(ld => ld.LoanId)
                .OnDelete(DeleteBehavior.Cascade);

            // BookCopy (1) - LoanDetail (nhiều): dùng Restrict để KHÔNG cho xóa 1 đầu sách
            // nếu đầu sách đó đã từng xuất hiện trong lịch sử mượn (bảo toàn dữ liệu lịch sử)
            modelBuilder.Entity<LoanDetail>()
                .HasOne(ld => ld.BookCopy)
                .WithMany()
                .HasForeignKey(ld => ld.BookCopyId)
                .OnDelete(DeleteBehavior.Restrict);

            // LoanDetail (1) - CompensationCase (nhiều, thực tế thường chỉ 1): xóa dòng mượn
            // thì xóa luôn case bồi thường phát sinh từ dòng đó
            modelBuilder.Entity<CompensationCase>()
                .HasOne(cc => cc.LoanDetail)
                .WithMany()
                .HasForeignKey(cc => cc.LoanDetailId)
                .OnDelete(DeleteBehavior.Cascade);

            // LibraryCard (1) - CompensationCase (nhiều): PHẢI dùng Restrict (không cascade).
            // Lý do: CompensationCase đã có 1 đường cascade gián tiếp tới LibraryCard qua
            // LoanDetail -> Loan -> LibraryCard (3 dòng cấu hình phía trên). Nếu để đường
            // trực tiếp này cũng Cascade thì sẽ có 2 đường cascade cùng trỏ tới LibraryCard,
            // SQL Server sẽ từ chối tạo constraint với lỗi "may cause cycles or multiple
            // cascade paths" khi chạy migration.
            modelBuilder.Entity<CompensationCase>()
                .HasOne(cc => cc.LibraryCard)
                .WithMany()
                .HasForeignKey(cc => cc.CardNumber)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
