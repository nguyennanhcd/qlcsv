using QLCSV.Validation;
using System.ComponentModel.DataAnnotations;

namespace QLCSV.DTOs.Alumni
{
    public class AlumniUpdateRequest
    {
        [StringLength(100, ErrorMessage = "Chức vụ hiện tại tối đa 100 ký tự")]
        public string? CurrentPosition { get; set; }

        [StringLength(150, ErrorMessage = "Tên công ty tối đa 150 ký tự")]
        public string? Company { get; set; }

        [StringLength(100, ErrorMessage = "Ngành nghề công ty tối đa 100 ký tự")]
        public string? CompanyIndustry { get; set; }

        [StringLength(100, ErrorMessage = "Thành phố tối đa 100 ký tự")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "Quốc gia tối đa 100 ký tự")]
        public string? Country { get; set; }

        [ValidUrl]
        [StringLength(250, ErrorMessage = "Link LinkedIn tối đa 250 ký tự")]
        public string? LinkedinUrl { get; set; }

        [ValidUrl]
        [StringLength(250, ErrorMessage = "Link Facebook tối đa 250 ký tự")]
        public string? FacebookUrl { get; set; }

        [StringLength(1000, ErrorMessage = "Giới thiệu tối đa 1000 ký tự")]
        public string? Bio { get; set; }

        // Cho phép update IsPublic luôn trong request (optional)
        public bool? IsPublic { get; set; }
    }
}
