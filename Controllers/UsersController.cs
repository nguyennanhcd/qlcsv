using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCSV.Data;
using QLCSV.DTOs;
using QLCSV.DTOs.User;
using System.Security.Claims;

namespace QLCSV.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")] // toàn bộ controller chỉ cho admin
    public class UsersController : BaseController
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }


        // GET: /api/users?role=alumni&isActive=true&search=abc&pageNumber=1&pageSize=20
        [HttpGet]
        public async Task<ActionResult<PagedResult<UserResponse>>> GetUsers(
            [FromQuery] string? role,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
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

            var totalCount = await query.CountAsync();
            pageSize = Math.Min(pageSize, 100);
            pageNumber = Math.Max(pageNumber, 1);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userResponses = users.Select(u => new UserResponse
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
                StudentId = u.AlumniProfile?.StudentId,
                GraduationYear = u.AlumniProfile?.GraduationYear,

                FacultyId = u.AlumniProfile?.FacultyId,
                FacultyName = u.AlumniProfile?.Faculty?.Name,

                MajorId = u.AlumniProfile?.MajorId,
                MajorName = u.AlumniProfile?.Major?.Name
            }).ToList();

            return Ok(new PagedResult<UserResponse>
            {
                Items = userResponses,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
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
                FacultyName = user.AlumniProfile?.Faculty?.Name ?? null,

                MajorId = user.AlumniProfile?.MajorId,
                MajorName = user.AlumniProfile?.Major?.Name ?? null
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
