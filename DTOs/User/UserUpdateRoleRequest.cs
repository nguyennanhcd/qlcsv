using System.ComponentModel.DataAnnotations;

namespace QLCSV.DTOs.User
{
    public class UserUpdateRoleRequest
    {
        // alumni | admin | pending
        [Required(ErrorMessage = "Vai trò không được để trống")]
        [RegularExpression("^(alumni|admin|pending)$",
            ErrorMessage = "Role không hợp lệ. Chỉ cho phép: alumni, admin, pending")]
        public string Role { get; set; } = null!;
    }
}
