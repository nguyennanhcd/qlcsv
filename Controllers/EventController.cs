using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCSV.Data;
using QLCSV.DTOs;
using QLCSV.DTOs.Event;
using System.Security.Claims;

namespace QLCSV.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : BaseController
    {
        private readonly AppDbContext _context;

        public EventsController(AppDbContext context)
        {
            _context = context;
        }

        // ===================== PUBLIC: LIST & DETAIL =====================

        // GET: /api/events
        // Filter: onlyUpcoming, from, to, keyword
        [HttpGet]
        public async Task<ActionResult<PagedResult<EventResponse>>> GetEvents(
            [FromQuery] bool onlyUpcoming = true,
            [FromQuery] DateTimeOffset? from = null,
            [FromQuery] DateTimeOffset? to = null,
            [FromQuery] string? keyword = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var now = DateTimeOffset.UtcNow;

            var query = _context.Events
                .Include(e => e.Registrations)
                .Include(e => e.CreatedByUser)
                .AsQueryable();

            if (onlyUpcoming)
            {
                query = query.Where(e => e.EventDate >= now);
            }

            if (from.HasValue)
            {
                query = query.Where(e => e.EventDate >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(e => e.EventDate <= to.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(e =>
                    EF.Functions.ILike(e.Title, $"%{keyword}%") ||
                    (e.Description != null && EF.Functions.ILike(e.Description, $"%{keyword}%")));
            }

            var currentUserId = GetCurrentUserId();

            var totalCount = await query.CountAsync();
            pageSize = Math.Min(pageSize, 100);
            pageNumber = Math.Max(pageNumber, 1);

            var events = await query
                .OrderBy(e => e.EventDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventResponse
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    IsOnline = e.IsOnline,
                    MeetLink = e.MeetLink,
                    ThumbnailUrl = e.ThumbnailUrl,
                    CreatedBy = e.CreatedBy,
                    CreatedByName = e.CreatedByUser.FullName,
                    CreatedAt = e.CreatedAt,
                    MaxParticipants = e.MaxParticipants,
                    RegisteredCount = e.Registrations.Count(r => r.Status == "registered"),
                    MyRegistrationStatus = currentUserId == null
                        ? null
                        : e.Registrations
                            .Where(r => r.UserId == currentUserId.Value)
                            .Select(r => r.Status)
                            .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new PagedResult<EventResponse>
            {
                Items = events,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        // GET: /api/events/{id}
        [HttpGet("{id:long}")]
        public async Task<ActionResult<EventResponse>> GetEventById(long id)
        {
            var currentUserId = GetCurrentUserId();

            var ev = await _context.Events
                .Include(e => e.Registrations)
                .Include(e => e.CreatedByUser)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound(new { Message = "Sự kiện không tồn tại" });

            var response = new EventResponse
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                IsOnline = ev.IsOnline,
                MeetLink = ev.IsOnline ? ev.MeetLink : null,
                ThumbnailUrl = ev.ThumbnailUrl,
                CreatedBy = ev.CreatedBy,
                CreatedByName = ev.CreatedByUser.FullName,
                CreatedAt = ev.CreatedAt,
                MaxParticipants = ev.MaxParticipants,
                RegisteredCount = ev.Registrations.Count(r => r.Status == "registered"),
                MyRegistrationStatus = currentUserId == null
                    ? null
                    : ev.Registrations
                        .Where(r => r.UserId == currentUserId.Value)
                        .Select(r => r.Status)
                        .FirstOrDefault()
            };

            return Ok(response);
        }

        // ===================== ADMIN: CRUD EVENT =====================

        // POST: /api/events  (ADMIN)
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<EventResponse>> CreateEvent(
            [FromBody] EventCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            // Optional rule: nếu IsOnline = true mà không có MeetLink thì cho tạo nhưng frontend nên validate thêm.

            var ev = new QLCSV.Models.Event
            {
                Title = request.Title,
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location,
                IsOnline = request.IsOnline,
                MeetLink = request.MeetLink,
                ThumbnailUrl = request.ThumbnailUrl,
                MaxParticipants = request.MaxParticipants,
                CreatedBy = userId.Value,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            // Load lại CreatedByUser
            await _context.Entry(ev).Reference(e => e.CreatedByUser).LoadAsync();

            var response = new EventResponse
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                IsOnline = ev.IsOnline,
                MeetLink = ev.MeetLink,
                ThumbnailUrl = ev.ThumbnailUrl,
                CreatedBy = ev.CreatedBy,
                CreatedByName = ev.CreatedByUser.FullName,
                CreatedAt = ev.CreatedAt,
                MaxParticipants = ev.MaxParticipants,
                RegisteredCount = 0,
                MyRegistrationStatus = null
            };

            return CreatedAtAction(nameof(GetEventById), new { id = ev.Id }, response);
        }

        // PUT: /api/events/{id}  (ADMIN)
        [Authorize(Roles = "admin")]
        [HttpPut("{id:long}")]
        public async Task<ActionResult<EventResponse>> UpdateEvent(
            long id,
            [FromBody] EventUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ev = await _context.Events
                .Include(e => e.Registrations)
                .Include(e => e.CreatedByUser)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound(new { Message = "Sự kiện không tồn tại" });

            ev.Title = request.Title;
            ev.Description = request.Description;
            ev.EventDate = request.EventDate;
            ev.Location = request.Location;
            ev.IsOnline = request.IsOnline;
            ev.MeetLink = request.MeetLink;
            ev.ThumbnailUrl = request.ThumbnailUrl;
            ev.MaxParticipants = request.MaxParticipants;

            await _context.SaveChangesAsync();

            var currentUserId = GetCurrentUserId();

            var response = new EventResponse
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                IsOnline = ev.IsOnline,
                MeetLink = ev.MeetLink,
                ThumbnailUrl = ev.ThumbnailUrl,
                CreatedBy = ev.CreatedBy,
                CreatedByName = ev.CreatedByUser.FullName,
                CreatedAt = ev.CreatedAt,
                MaxParticipants = ev.MaxParticipants,
                RegisteredCount = ev.Registrations.Count(r => r.Status == "registered"),
                MyRegistrationStatus = currentUserId == null
                    ? null
                    : ev.Registrations
                        .Where(r => r.UserId == currentUserId.Value)
                        .Select(r => r.Status)
                        .FirstOrDefault()
            };

            return Ok(response);
        }

        // DELETE: /api/events/{id}  (ADMIN)
        [Authorize(Roles = "admin")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteEvent(long id)
        {
            var ev = await _context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound(new { Message = "Sự kiện không tồn tại" });

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Xóa sự kiện thành công" });
        }

        // ===================== USER REGISTRATION =====================

        // POST: /api/events/{id}/register
        [Authorize]
        [HttpPost("{id:long}/register")]
        public async Task<IActionResult> RegisterEvent(long id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            // Use transaction and row-level locking to prevent race conditions
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ev = await _context.Events
                    .Include(e => e.Registrations)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (ev == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound(new { Message = "Sự kiện không tồn tại" });
                }

                if (ev.EventDate < DateTimeOffset.UtcNow)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { Message = "Sự kiện đã diễn ra, không thể đăng ký" });
                }

                // Check existing registration first
                var existing = await _context.EventRegistrations
                    .FirstOrDefaultAsync(r => r.EventId == id && r.UserId == userId.Value);

                if (existing != null && existing.Status == "registered")
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { Message = "Bạn đã đăng ký sự kiện này rồi" });
                }

                // Atomic check for capacity with proper counting
                if (ev.MaxParticipants.HasValue)
                {
                    var currentCount = await _context.EventRegistrations
                        .CountAsync(r => r.EventId == id && r.Status == "registered");
                    
                    if (currentCount >= ev.MaxParticipants.Value)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { Message = "Sự kiện đã đủ số lượng" });
                    }
                }

                if (existing == null)
                {
                    existing = new QLCSV.Models.EventRegistration
                    {
                        EventId = id,
                        UserId = userId.Value,
                        RegisteredAt = DateTimeOffset.UtcNow,
                        Status = "registered"
                    };
                    _context.EventRegistrations.Add(existing);
                }
                else
                {
                    existing.Status = "registered";
                    existing.RegisteredAt = DateTimeOffset.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = "Đăng ký sự kiện thành công" });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // POST: /api/events/{id}/cancel
        [Authorize]
        [HttpPost("{id:long}/cancel")]
        public async Task<IActionResult> CancelRegistration(long id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var registration = await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == id && r.UserId == userId.Value);

            if (registration == null || registration.Status != "registered")
                return BadRequest(new { Message = "Bạn chưa đăng ký sự kiện này hoặc đã hủy trước đó" });

            registration.Status = "cancelled";
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Hủy đăng ký thành công" });
        }

        // GET: /api/events/my-registrations
        [Authorize]
        [HttpGet("my-registrations")]
        public async Task<ActionResult<IEnumerable<MyEventRegistrationResponse>>> GetMyRegistrations()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var regs = await _context.EventRegistrations
                .Include(r => r.Event)
                .Where(r => r.UserId == userId.Value)
                .OrderByDescending(r => r.RegisteredAt)
                .Select(r => new MyEventRegistrationResponse
                {
                    EventId = r.EventId,
                    Title = r.Event.Title,
                    EventDate = r.Event.EventDate,
                    Location = r.Event.Location,
                    IsOnline = r.Event.IsOnline,
                    ThumbnailUrl = r.Event.ThumbnailUrl,
                    RegisteredAt = r.RegisteredAt,
                    Status = r.Status
                })
                .ToListAsync();

            return Ok(regs);
        }

        // ===================== ADMIN: VIEW REGISTRATIONS =====================

        // GET: /api/events/{id}/registrations  (ADMIN)
        [Authorize(Roles = "admin")]
        [HttpGet("{id:long}/registrations")]
        public async Task<ActionResult<PagedResult<EventRegistrationUserResponse>>> GetRegistrationsByEvent(
            long id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var exists = await _context.Events.AnyAsync(e => e.Id == id);
            if (!exists)
                return NotFound(new { Message = "Sự kiện không tồn tại" });

            var query = _context.EventRegistrations
                .Include(r => r.User)
                .Where(r => r.EventId == id);

            var totalCount = await query.CountAsync();
            pageSize = Math.Min(pageSize, 200);
            pageNumber = Math.Max(pageNumber, 1);

            var regs = await query
                .OrderBy(r => r.RegisteredAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new EventRegistrationUserResponse
                {
                    UserId = r.UserId,
                    FullName = r.User.FullName,
                    Email = r.User.Email,
                    RegisteredAt = r.RegisteredAt,
                    Status = r.Status
                })
                .ToListAsync();

            return Ok(new PagedResult<EventRegistrationUserResponse>
            {
                Items = regs,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
    }
}
