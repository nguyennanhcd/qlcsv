using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCSV.Data;
using QLCSV.DTOs;
using QLCSV.DTOs.Alumni;
using System.Security.Claims;

namespace QLCSV.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlumniController : BaseController
    {
        private readonly AppDbContext _context;

        public AlumniController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/alumni
        // Public danh sách cựu sinh viên, chỉ lấy IsPublic = true
        // Filter theo facultyId, majorId, graduationYear, city
        [HttpGet]
        public async Task<ActionResult<PagedResult<AlumniPublicResponse>>> GetPublicAlumni(
            [FromQuery] int? facultyId,
            [FromQuery] int? majorId,
            [FromQuery] int? graduationYear,
            [FromQuery] string? city,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.AlumniProfiles
                .Include(a => a.User)
                .Include(a => a.Faculty)
                .Include(a => a.Major)
                .Where(a => a.IsPublic)
                .AsQueryable();

            if (facultyId.HasValue)
                query = query.Where(a => a.FacultyId == facultyId.Value);

            if (majorId.HasValue)
                query = query.Where(a => a.MajorId == majorId.Value);

            if (graduationYear.HasValue)
                query = query.Where(a => a.GraduationYear == graduationYear.Value);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(a => a.City != null && EF.Functions.ILike(a.City, $"%{city}%"));

            var totalCount = await query.CountAsync();
            pageSize = Math.Min(pageSize, 100);
            pageNumber = Math.Max(pageNumber, 1);

            var result = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AlumniPublicResponse
                {
                    Id = a.Id,
                    FullName = a.User.FullName,
                    AvatarUrl = a.User.AvatarUrl,
                    StudentId = a.StudentId,
                    GraduationYear = a.GraduationYear,
                    FacultyId = a.FacultyId,
                    FacultyName = a.Faculty.Name,
                    MajorId = a.MajorId,
                    MajorName = a.Major.Name,
                    CurrentPosition = a.CurrentPosition,
                    Company = a.Company,
                    City = a.City,
                    Country = a.Country,
                    IsPublic = a.IsPublic
                })
                .ToListAsync();

            return Ok(new PagedResult<AlumniPublicResponse>
            {
                Items = result,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        // GET: /api/alumni/{id}
        // Nếu profile IsPublic => ai cũng xem được
        // Nếu không public => chỉ chủ nhân hoặc admin xem được
        [HttpGet("{id:long}")]
        public async Task<ActionResult<AlumniDetailResponse>> GetAlumniById(long id)
        {
            var profile = await _context.AlumniProfiles
                .Include(a => a.User)
                .Include(a => a.Faculty)
                .Include(a => a.Major)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (profile == null)
                return NotFound(new { Message = "Hồ sơ không tồn tại" });

            var currentUserId = GetCurrentUserId();
            var isAdmin = User.IsInRole("admin");

            if (!profile.IsPublic && currentUserId != profile.UserId && !isAdmin)
            {
                return Forbid();
            }

            var response = MapToDetailResponse(profile);
            return Ok(response);
        }

        // GET: /api/alumni/me
        // Lấy hồ sơ chi tiết của chính mình
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<AlumniDetailResponse>> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var profile = await _context.AlumniProfiles
                .Include(a => a.User)
                .Include(a => a.Faculty)
                .Include(a => a.Major)
                .FirstOrDefaultAsync(a => a.UserId == userId.Value);

            if (profile == null)
                return NotFound(new { Message = "Bạn chưa hoàn thiện hồ sơ" });

            var response = MapToDetailResponse(profile);
            return Ok(response);
        }

        // PUT: /api/alumni/me
        // Cập nhật hồ sơ của chính mình
        [Authorize]
        [HttpPut("me")]
        public async Task<ActionResult<AlumniDetailResponse>> UpdateMyProfile(
            [FromBody] AlumniUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var profile = await _context.AlumniProfiles
                .Include(a => a.User)
                .Include(a => a.Faculty)
                .Include(a => a.Major)
                .FirstOrDefaultAsync(a => a.UserId == userId.Value);

            if (profile == null)
                return NotFound(new { Message = "Bạn chưa hoàn thiện hồ sơ" });

            // Update các field nếu có gửi lên
            if (request.CurrentPosition != null)
                profile.CurrentPosition = request.CurrentPosition;

            if (request.Company != null)
                profile.Company = request.Company;

            if (request.CompanyIndustry != null)
                profile.CompanyIndustry = request.CompanyIndustry;

            if (request.City != null)
                profile.City = request.City;

            if (request.Country != null)
                profile.Country = request.Country;

            if (request.LinkedinUrl != null)
                profile.LinkedinUrl = request.LinkedinUrl;

            if (request.FacebookUrl != null)
                profile.FacebookUrl = request.FacebookUrl;

            if (request.Bio != null)
                profile.Bio = request.Bio;

            if (request.IsPublic.HasValue)
                profile.IsPublic = request.IsPublic.Value;

            profile.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            var response = MapToDetailResponse(profile);
            return Ok(response);
        }

        // PUT: /api/alumni/me/privacy
        // Toggle nhanh IsPublic
        [Authorize]
        [HttpPut("me/privacy")]
        public async Task<IActionResult> UpdateMyPrivacy(
            [FromBody] AlumniPrivacyUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var profile = await _context.AlumniProfiles
                .FirstOrDefaultAsync(a => a.UserId == userId.Value);

            if (profile == null)
                return NotFound(new { Message = "Bạn chưa hoàn thiện hồ sơ" });

            profile.IsPublic = request.IsPublic;
            profile.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cập nhật quyền hiển thị thành công", profile.IsPublic });
        }

        // Helper: map từ entity sang DTO
        private static AlumniDetailResponse MapToDetailResponse(QLCSV.Models.AlumniProfile profile)
        {
            return new AlumniDetailResponse
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.User.FullName,
                Email = profile.User.Email,
                AvatarUrl = profile.User.AvatarUrl,

                StudentId = profile.StudentId,
                GraduationYear = profile.GraduationYear,

                FacultyId = profile.FacultyId,
                FacultyName = profile.Faculty.Name,

                MajorId = profile.MajorId,
                MajorName = profile.Major.Name,

                CurrentPosition = profile.CurrentPosition,
                Company = profile.Company,
                CompanyIndustry = profile.CompanyIndustry,
                City = profile.City,
                Country = profile.Country,
                LinkedinUrl = profile.LinkedinUrl,
                FacebookUrl = profile.FacebookUrl,
                Bio = profile.Bio,

                IsPublic = profile.IsPublic,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }
    }
}
