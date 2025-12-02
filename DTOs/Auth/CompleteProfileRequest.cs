using System.ComponentModel.DataAnnotations;

namespace QLCSV.DTOs.Auth
{
    public class CompleteProfileRequest
    {
        [Required(ErrorMessage = "Mã sinh viên không được để trống")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Mã sinh viên phải từ 5-20 ký tự")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Mã sinh viên chỉ bao gồm chữ hoa và số")]
        public string StudentId { get; set; } = null!;

        [Required(ErrorMessage = "Năm tốt nghiệp không được để trống")]
        [Range(1950, 2100, ErrorMessage = "Năm tốt nghiệp không hợp lệ")]
        public int GraduationYear { get; set; }

        [Required(ErrorMessage = "Khoa không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Khoa không hợp lệ")]
        public int FacultyId { get; set; }

        [Required(ErrorMessage = "Ngành không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Ngành không hợp lệ")]
        public int MajorId { get; set; }
    }
}