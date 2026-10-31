using College_Web_Application.Data;
using College_Web_Application.Models;
using College_Web_Application.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace College_Web_Application.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Principal,Clerk")]
    public class FeeChallansController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FeeChallansController> _logger;

        public FeeChallansController(ApplicationDbContext context, ILogger<FeeChallansController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/FeeChallans
        public async Task<IActionResult> Index(
            string? searchQuery,
            int? programId,
            int? semester,
            string? statusFilter,
            int page = 1)
        {
            const int pageSize = 50;
            if (page < 1) page = 1;

            var query = _context.FeeChallans
                .AsNoTracking()
                .Include(f => f.Student)
                    .ThenInclude(s => s!.Program)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var q = searchQuery.Trim().ToLower();
                query = query.Where(f =>
                    f.ChallanNumber.ToLower().Contains(q) ||
                    (f.BankReference != null && f.BankReference.ToLower().Contains(q)) ||
                    (f.Student != null && (
                        f.Student.FullName.ToLower().Contains(q) ||
                        f.Student.RollNumber.ToLower().Contains(q) ||
                        f.Student.CNIC.Contains(q))));
            }

            if (programId.HasValue && programId.Value > 0)
            {
                query = query.Where(f => f.Student != null && f.Student.ProgramId == programId.Value);
            }

            if (semester.HasValue && semester.Value > 0)
            {
                query = query.Where(f => f.Semester == semester.Value);
            }

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                if (statusFilter == "Paid")
                {
                    query = query.Where(f => f.IsPaid);
                }
                else if (statusFilter == "Unpaid")
                {
                    query = query.Where(f => !f.IsPaid);
                }
            }

            int totalCount = await query.CountAsync();

            var challans = await query
                .OrderByDescending(f => f.IssueDate)
                .ThenByDescending(f => f.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new FeeChallanListViewModel
            {
                Challans = challans,
                SearchQuery = searchQuery,
                ProgramId = programId,
                Semester = semester,
                StatusFilter = statusFilter ?? "All",
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Programs = await _context.AcademicPrograms.AsNoTracking().OrderBy(p => p.Title).ToListAsync()
            };

            return View(viewModel);
        }

        // POST: Admin/FeeChallans/GenerateBulkChallans
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateBulkChallans(BulkChallanGenerateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid bulk challan parameters provided.";
                return RedirectToAction(nameof(Index));
            }

            // Find all active students in this program and semester
            var students = await _context.StudentProfiles
                .Where(s => s.ProgramId == model.ProgramId && s.CurrentSemester == model.Semester && s.IsActive)
                .ToListAsync();

            if (!students.Any())
            {
                TempData["ErrorMessage"] = "No active students found in the selected Program and Semester.";
                return RedirectToAction(nameof(Index));
            }

            int existingCount = await _context.FeeChallans.CountAsync();
            var newChallans = new List<FeeChallan>();
            var currentYear = DateTime.UtcNow.Year;

            decimal total = model.TuitionFee + model.AdmissionFee + model.ExamFee + model.LibraryFund + model.LabFund;

            foreach (var student in students)
            {
                // Check if challan already exists for this semester
                bool alreadyGenerated = await _context.FeeChallans.AnyAsync(f => f.StudentId == student.Id && f.Semester == model.Semester);
                if (alreadyGenerated) continue;

                existingCount++;
                var challan = new FeeChallan
                {
                    ChallanNumber = $"BOK-GDCM-{currentYear}-{existingCount:D4}",
                    StudentId = student.Id,
                    Semester = model.Semester,
                    TuitionFee = model.TuitionFee,
                    AdmissionFee = model.AdmissionFee,
                    ExamFee = model.ExamFee,
                    LibraryFund = model.LibraryFund,
                    LabFund = model.LabFund,
                    LateFine = 500.00m,
                    TotalAmount = total,
                    IssueDate = DateTime.UtcNow,
                    DueDate = model.DueDate > DateTime.UtcNow ? model.DueDate : DateTime.UtcNow.AddDays(15),
                    IsPaid = false
                };
                newChallans.Add(challan);
            }

            if (newChallans.Any())
            {
                await _context.FeeChallans.AddRangeAsync(newChallans);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully generated {newChallans.Count} Bank of Khyber (BoK) fee vouchers!";
            }
            else
            {
                TempData["ErrorMessage"] = "All students in this batch already have fee challans generated for this semester.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/FeeChallans/MarkAsPaid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(FeePaymentVerificationViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ChallanNumber))
            {
                TempData["ErrorMessage"] = "Challan number is required for verification.";
                return RedirectToAction(nameof(Index));
            }

            var challan = await _context.FeeChallans
                .Include(f => f.Student)
                .FirstOrDefaultAsync(f => f.ChallanNumber.ToLower() == model.ChallanNumber.Trim().ToLower());

            if (challan == null)
            {
                TempData["ErrorMessage"] = $"Challan number '{model.ChallanNumber}' not found in college database.";
                return RedirectToAction(nameof(Index));
            }

            if (challan.IsPaid)
            {
                TempData["ErrorMessage"] = $"Challan '{challan.ChallanNumber}' is already verified as paid on {challan.PaidDate:dd MMM yyyy}.";
                return RedirectToAction(nameof(Index));
            }

            challan.IsPaid = true;
            challan.PaidDate = model.PaidDate;
            challan.BankReference = model.BankReference;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Fee Challan '{challan.ChallanNumber}' for {challan.Student?.FullName} verified and marked PAID!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/FeeChallans/PrintChallan/5
        public async Task<IActionResult> PrintChallan(int id)
        {
            var challan = await _context.FeeChallans
                .AsNoTracking()
                .Include(f => f.Student)
                    .ThenInclude(s => s!.Program)
                        .ThenInclude(p => p!.Department)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (challan == null)
            {
                TempData["ErrorMessage"] = "Fee challan record not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(challan);
        }
    }
}
