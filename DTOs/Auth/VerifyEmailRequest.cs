using System.ComponentModel.DataAnnotations;

namespace QLCSV.DTOs.Auth
{
    public class VerifyEmailRequest
    {
        [Required(ErrorMessage = "Token không được để trống")]
        public string Token { get; set; } = null!;
    }
}
