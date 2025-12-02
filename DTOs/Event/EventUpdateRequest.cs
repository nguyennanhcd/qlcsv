using System.ComponentModel.DataAnnotations;

namespace QLCSV.DTOs.Event
{
    public class EventUpdateRequest
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(255, ErrorMessage = "Tiêu đề tối đa 255 ký tự")]
        public string Title { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Thời gian diễn ra không được để trống")]
        public DateTimeOffset EventDate { get; set; }

        [StringLength(255, ErrorMessage = "Địa điểm tối đa 255 ký tự")]
        public string? Location { get; set; }

        public bool IsOnline { get; set; } = false;

        [StringLength(500, ErrorMessage = "Link họp tối đa 500 ký tự")]
        public string? MeetLink { get; set; }

        [StringLength(500, ErrorMessage = "Link ảnh tối đa 500 ký tự")]
        public string? ThumbnailUrl { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng tối đa phải lớn hơn 0")]
        public int? MaxParticipants { get; set; }
    }
}
