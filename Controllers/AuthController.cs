using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCSV.Data;
using QLCSV.DTOs.Auth;
using QLCSV.Service;
using QLCSV.Models;
using System.Security.Claims;

namespace QLCSV.Controllers.Auth
{
    // Đánh dấu đây là Web API controller
    [ApiController]
    [Route("api/[controller]")] // => /api/auth/...
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthController(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // POST: /api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return Conflict(new RegisterResponse { Success = false, Message = "Email đã được sử dụng" });

            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "pending",
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new RegisterResponse
            {
                Success = true,
                Message = "Đăng ký thành công! Vui lòng hoàn thiện hồ sơ.",
                Token = _jwtService.GenerateToken(user),
                UserId = user.Id
            });
        }

        // POST: /api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new LoginResponse { Success = false, Message = "Email hoặc mật khẩu sai" });

            if (!user.IsActive)
                return Unauthorized(new LoginResponse { Success = false, Message = "Tài khoản đã bị khóa" });

            bool profileCompleted = await _context.AlumniProfiles.AnyAsync(p => p.UserId == user.Id);

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = _jwtService.GenerateToken(user),
                Role = user.Role,
                ProfileCompleted = profileCompleted
            });
        }

        // POST: /api/auth/complete-profile
        [Authorize]
        [HttpPost("complete-profile")]
        public async Task<ActionResult<CompleteProfileResponse>> CompleteProfile([FromBody] CompleteProfileRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new CompleteProfileResponse { Success = false, Message = "Token không hợp lệ" });
            }

            if (await _context.AlumniProfiles.AnyAsync(p => p.UserId == userId))
                return BadRequest(new CompleteProfileResponse { Success = false, Message = "Bạn đã hoàn thiện hồ sơ rồi" });

            if (await _context.AlumniProfiles.AnyAsync(p => p.StudentId == request.StudentId))
                return Conflict(new CompleteProfileResponse { Success = false, Message = "Mã sinh viên đã được sử dụng" });

            if (!await _context.Faculties.AnyAsync(f => f.Id == request.FacultyId) ||
                !await _context.Majors.AnyAsync(m => m.Id == request.MajorId))
                return BadRequest(new CompleteProfileResponse { Success = false, Message = "Khoa hoặc ngành không tồn tại" });

            var profile = new AlumniProfile
            {
                UserId = userId,
                StudentId = request.StudentId,
                GraduationYear = request.GraduationYear,
                FacultyId = request.FacultyId,
                MajorId = request.MajorId,
                // Có thể bỏ 2 dòng dưới vì đã set default trong model, nhưng để lại cũng không sao
                Country = "Việt Nam",
                IsPublic = false
            };

            var user = await _context.Users.FindAsync(userId);
            if (user != null) user.Role = "alumni";

            _context.AlumniProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return Ok(new CompleteProfileResponse
            {
                Success = true,
                Message = "Hoàn thiện hồ sơ thành công! Đang chờ admin duyệt."
            });
        }

        // GET: /api/auth/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.AlumniProfile!)
                    .ThenInclude(p => p.Faculty)
                .Include(u => u.AlumniProfile!)
                    .ThenInclude(p => p.Major)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                Profile = user.AlumniProfile == null ? null : new
                {
                    user.AlumniProfile.StudentId,
                    user.AlumniProfile.GraduationYear,
                    FacultyName = user.AlumniProfile.Faculty?.Name,
                    MajorName = user.AlumniProfile.Major?.Name,
                    user.AlumniProfile.CurrentPosition,
                    user.AlumniProfile.Company,
                    user.AlumniProfile.City,
                    user.AlumniProfile.IsPublic
                }
            });
        }

        // TEMPORARY: Create first admin user - REMOVE AFTER USE!
        [HttpPost("create-admin")]
        public async Task<ActionResult> CreateAdmin([FromBody] RegisterRequest request)
        {
            // Check if any admin exists
            if (await _context.Users.AnyAsync(u => u.Role == "admin"))
                return BadRequest(new { success = false, message = "Admin already exists" });

            // Check if email is already used
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return Conflict(new { success = false, message = "Email already in use" });

            var admin = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "admin",
                IsActive = true
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Admin created successfully! PLEASE REMOVE THIS ENDPOINT NOW!",
                userId = admin.Id,
                email = admin.Email,
                role = admin.Role
            });
        }
    }
}
