using QLCSV.Validation;
using System.ComponentModel.DataAnnotations;

namespace QLCSV.DTOs.Auth
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Token không được để trống")]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StrongPassword]
        public string NewPassword { get; set; } = null!;
    }
}
