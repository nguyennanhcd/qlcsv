using QLCSV.Validation;
using System.ComponentModel.DataAnnotations;

namespace QLCSV.DTOs.Event
{
    public class EventCreateRequest
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Tiêu đề phải từ 5-255 ký tự")]
        public string Title { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Thời gian diễn ra không được để trống")]
        public DateTimeOffset EventDate { get; set; }

        [StringLength(255, ErrorMessage = "Địa điểm tối đa 255 ký tự")]
        public string? Location { get; set; }

        public bool IsOnline { get; set; } = false;

        [RequiredIfOnline(nameof(IsOnline), ErrorMessage = "Link họp không được để trống cho sự kiện trực tuyến")]
        [ValidUrl]
        [StringLength(500, ErrorMessage = "Link họp tối đa 500 ký tự")]
        public string? MeetLink { get; set; }

        [ValidUrl]
        [StringLength(500, ErrorMessage = "Link ảnh tối đa 500 ký tự")]
        public string? ThumbnailUrl { get; set; }

        [Range(1, 10000, ErrorMessage = "Số lượng tối đa phải từ 1-10000")]
        public int? MaxParticipants { get; set; }
    }
}
