using System.ComponentModel.DataAnnotations;

namespace QLCSV.DTOs.User
{
    public class UserUpdateStatusRequest
    {
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public bool IsActive { get; set; }
    }
}
