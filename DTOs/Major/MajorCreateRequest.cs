using System.ComponentModel.DataAnnotations;

namespace QLCSV.DTOs.Major
{
    public class MajorCreateRequest
    {
        [Required(ErrorMessage = "Khoa không được để trống")]
        public int FacultyId { get; set; }

        [Required(ErrorMessage = "Tên ngành không được để trống")]
        [StringLength(100, ErrorMessage = "Tên ngành không được quá 100 ký tự")]
        public string Name { get; set; } = null!;

        [StringLength(20, ErrorMessage = "Mã ngành tối đa 20 ký tự")]
        public string? Code { get; set; }
    }
}
