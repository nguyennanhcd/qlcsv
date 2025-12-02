using System.ComponentModel.DataAnnotations;

namespace QLCSV.DTOs.Faculty
{
    public class FacultyUpdateRequest
    {
        [Required(ErrorMessage = "Tên khoa không được để trống")]
        [StringLength(100, ErrorMessage = "Tên khoa không được quá 100 ký tự")]
        public string Name { get; set; } = null!;

        [StringLength(20, ErrorMessage = "Tên viết tắt tối đa 20 ký tự")]
        public string? ShortName { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        public string? Description { get; set; }
    }
}
