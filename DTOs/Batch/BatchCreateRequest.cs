using System.ComponentModel.DataAnnotations;

namespace QLCSV.DTOs.Batch
{
    public class BatchCreateRequest
    {
        [Required(ErrorMessage = "Năm tốt nghiệp không được để trống")]
        [Range(1900, 2100, ErrorMessage = "Năm tốt nghiệp không hợp lệ")]
        public int GraduationYear { get; set; }

        [Required(ErrorMessage = "Tên khóa không được để trống")]
        [StringLength(100, ErrorMessage = "Tên khóa không được quá 100 ký tự")]
        public string Name { get; set; } = null!;

        [Range(1900, 2100, ErrorMessage = "Năm bắt đầu không hợp lệ")]
        public int? StartYear { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        public string? Description { get; set; }
    }
}
    