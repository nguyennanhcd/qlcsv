using QLCSV.Validation;
using System.ComponentModel.DataAnnotations;

namespace QLCSV.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255, ErrorMessage = "Email tối đa 255 ký tự")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StrongPassword]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2-255 ký tự")]
        public string FullName { get; set; } = null!;
    }
}