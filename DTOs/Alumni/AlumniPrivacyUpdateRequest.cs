using System.ComponentModel.DataAnnotations;

namespace QLCSV.DTOs.Alumni
{
    public class AlumniPrivacyUpdateRequest
    {
        [Required(ErrorMessage = "Trạng thái hiển thị không được để trống")]
        public bool IsPublic { get; set; }
    }
}
