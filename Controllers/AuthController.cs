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
        private readonly IEmailService _emailService;

        public AuthController(AppDbContext context, IJwtService jwtService, IEmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        // POST: /api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return Conflict(new RegisterResponse { Success = false, Message = "Email đã được sử dụng" });

            var verificationToken = Guid.NewGuid().ToString("N");

            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "pending",
                IsActive = true,
                EmailVerified = false,
                EmailVerificationToken = verificationToken,
                EmailVerificationTokenExpiry = DateTimeOffset.UtcNow.AddHours(24)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send verification email (don't wait, run in background)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailVerificationAsync(user.Email, user.FullName, verificationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send verification email: {ex.Message}");
                }
            });

            return Ok(new RegisterResponse
            {
                Success = true,
                Message = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.",
                Token = null, // No token until email verified
                UserId = user.Id
            });
        }

        // POST: /api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            // Prevent timing attack: always verify password even if user doesn't exist
            var passwordValid = user != null && BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            
            if (user == null || !passwordValid)
                return Unauthorized(new LoginResponse { Success = false, Message = "Email hoặc mật khẩu sai" });

            if (!user.IsActive)
                return Unauthorized(new LoginResponse { Success = false, Message = "Tài khoản đã bị khóa" });

            if (!user.EmailVerified)
                return Unauthorized(new LoginResponse { Success = false, Message = "Vui lòng xác thực email trước khi đăng nhập. Kiểm tra hộp thư của bạn." });

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

            // Use transaction to ensure atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var profile = new AlumniProfile
                {
                    UserId = userId,
                    StudentId = request.StudentId,
                    GraduationYear = request.GraduationYear,
                    FacultyId = request.FacultyId,
                    MajorId = request.MajorId
                    // Default values (Country, IsPublic) are set in model
                };

                _context.AlumniProfiles.Add(profile);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new CompleteProfileResponse
                {
                    Success = true,
                    Message = "Hoàn thiện hồ sơ thành công! Role vẫn là 'pending' cho đến khi admin duyệt."
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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
                user.EmailVerified,
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

        // POST: /api/auth/verify-email
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == request.Token);

            if (user == null)
                return BadRequest(new { Success = false, Message = "Token không hợp lệ" });

            if (user.EmailVerified)
                return Ok(new { Success = true, Message = "Email đã được xác thực trước đó" });

            if (user.EmailVerificationTokenExpiry < DateTimeOffset.UtcNow)
                return BadRequest(new { Success = false, Message = "Token đã hết hạn. Vui lòng yêu cầu gửi lại email xác thực." });

            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Xác thực email thành công!" });
        }

        // POST: /api/auth/resend-verification
        [Authorize]
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { Success = false, Message = "User không tồn tại" });

            if (user.EmailVerified)
                return BadRequest(new { Success = false, Message = "Email đã được xác thực rồi" });

            var verificationToken = Guid.NewGuid().ToString("N");
            user.EmailVerificationToken = verificationToken;
            user.EmailVerificationTokenExpiry = DateTimeOffset.UtcNow.AddHours(24);
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            // Send verification email
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailVerificationAsync(user.Email, user.FullName, verificationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send verification email: {ex.Message}");
                }
            });

            return Ok(new { Success = true, Message = "Email xác thực đã được gửi lại" });
        }

        // POST: /api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            // Don't reveal if email exists or not (security best practice)
            if (user == null)
                return Ok(new { Success = true, Message = "Nếu email tồn tại, link đặt lại mật khẩu đã được gửi" });

            var resetToken = Guid.NewGuid().ToString("N");
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTimeOffset.UtcNow.AddHours(1);
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            // Send password reset email
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendPasswordResetAsync(user.Email, user.FullName, resetToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send password reset email: {ex.Message}");
                }
            });

            return Ok(new { Success = true, Message = "Nếu email tồn tại, link đặt lại mật khẩu đã được gửi" });
        }

        // POST: /api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);

            if (user == null)
                return BadRequest(new { Success = false, Message = "Token không hợp lệ" });

            if (user.PasswordResetTokenExpiry < DateTimeOffset.UtcNow)
                return BadRequest(new { Success = false, Message = "Token đã hết hạn. Vui lòng yêu cầu đặt lại mật khẩu mới." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Đặt lại mật khẩu thành công!" });
        }
    }
}
