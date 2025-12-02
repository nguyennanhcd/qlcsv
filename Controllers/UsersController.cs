using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCSV.Data;
using QLCSV.DTOs.User;
using System.Security.Claims;

namespace QLCSV.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")] // toàn bộ controller chỉ cho admin
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // Helper: lấy userId từ token
        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return null;
            if (!long.TryParse(userIdClaim, out var userId)) return null;
            return userId;
        }


        // GET: /api/users?role=alumni&isActive=true&search=abc
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers(
            [FromQuery] string? role,
            [FromQuery] bool? isActive,
            [FromQuery] string? search)
        {
            var query = _context.Users
                .Include(u => u.AlumniProfile)
                    .ThenInclude( p => p.Faculty)
                .Include(u => u.AlumniProfile)
                    .ThenInclude(p => p.Major)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role == role);
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(lower) ||
                    u.Email.ToLower().Contains(lower));
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    AvatarUrl = u.AvatarUrl,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,

                    HasProfile = u.AlumniProfile != null,
                    StudentId = u.AlumniProfile != null ? u.AlumniProfile.StudentId : null,
                    GraduationYear = u.AlumniProfile != null ? u.AlumniProfile.GraduationYear : (int?)null,

                    FacultyId = u.AlumniProfile != null ? u.AlumniProfile.FacultyId : (int?)null,
                    FacultyName = u.AlumniProfile != null ? u.AlumniProfile.Faculty.Name : null,

                    MajorId = u.AlumniProfile != null ? u.AlumniProfile.MajorId : (int?)null,
                    MajorName = u.AlumniProfile != null ? u.AlumniProfile.Major.Name : null
                })
                .ToListAsync();

            return Ok(users);
        }

        // ===================== 2. GET DETAIL =====================

        // GET: /api/users/{id}
        [HttpGet("{id:long}")]
        public async Task<ActionResult<UserResponse>> GetUserById(long id)
        {
            var user = await _context.Users
                .Include(u => u.AlumniProfile)
                    .ThenInclude(p => p.Faculty)
                .Include(u => u.AlumniProfile)
                    .ThenInclude(p => p.Major)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { Message = "User không tồn tại" });

            var response = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,

                HasProfile = user.AlumniProfile != null,
                StudentId = user.AlumniProfile?.StudentId,
                GraduationYear = user.AlumniProfile?.GraduationYear,

                FacultyId = user.AlumniProfile?.FacultyId,
                FacultyName = user.AlumniProfile?.Faculty?.Name,

                MajorId = user.AlumniProfile?.MajorId,
                MajorName = user.AlumniProfile?.Major?.Name
            };

            return Ok(response);
        }

        // ===================== 3. UPDATE ROLE =====================

        // PUT: /api/users/{id}/role
        [HttpPut("{id:long}/role")]
        public async Task<IActionResult> UpdateUserRole(
            long id,
            [FromBody] UserUpdateRoleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { Message = "User không tồn tại" });

            var currentUserId = GetCurrentUserId();

            // Không cho admin tự hạ role của chính mình (tránh tự khóa luôn admin cuối)
            if (currentUserId.HasValue && currentUserId.Value == id && request.Role != "admin")
            {
                return BadRequest(new { Message = "Không thể thay đổi role của chính mình thành không phải admin" });
            }

            user.Role = request.Role;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cập nhật vai trò thành công", user.Id, user.Role });
        }

        // ===================== 4. UPDATE STATUS =====================

        // PUT: /api/users/{id}/status
        [HttpPut("{id:long}/status")]
        public async Task<IActionResult> UpdateUserStatus(
            long id,
            [FromBody] UserUpdateStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { Message = "User không tồn tại" });

            var currentUserId = GetCurrentUserId();

            // Không cho admin tự khóa chính mình
            if (currentUserId.HasValue && currentUserId.Value == id && request.IsActive == false)
            {
                return BadRequest(new { Message = "Không thể tự khóa tài khoản admin đang đăng nhập" });
            }

            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cập nhật trạng thái thành công", user.Id, user.IsActive });
        }

        // ===================== 5. (OPTIONAL) DELETE USER =====================

        // DELETE: /api/users/{id}
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var user = await _context.Users
                .Include(u => u.AlumniProfile)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { Message = "User không tồn tại" });

            var currentUserId = GetCurrentUserId();
            if (currentUserId.HasValue && currentUserId.Value == id)
            {
                return BadRequest(new { Message = "Không thể tự xóa chính mình" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Xóa user thành công" });
        }
    }
}
