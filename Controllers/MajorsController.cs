using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCSV.Data;
using QLCSV.DTOs.Major;
using QLCSV.Models;

namespace QLCSV.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MajorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MajorsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/majors?facultyId=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MajorResponse>>> GetMajors(
            [FromQuery] int? facultyId)
        {
            var query = _context.Majors
                .Include(m => m.Faculty)
                .Include(m => m.AlumniProfiles)
                .AsQueryable();

            if (facultyId.HasValue)
            {
                query = query.Where(m => m.FacultyId == facultyId.Value);
            }

            var majors = await query
                .Select(m => new MajorResponse
                {
                    Id = m.Id,
                    FacultyId = m.FacultyId,
                    FacultyName = m.Faculty.Name,
                    Name = m.Name,
                    Code = m.Code,
                    AlumniCount = m.AlumniProfiles.Count
                })
                .ToListAsync();

            return Ok(majors);
        }

        // GET: /api/majors/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MajorResponse>> GetMajor(int id)
        {
            var major = await _context.Majors
                .Include(m => m.Faculty)
                .Include(m => m.AlumniProfiles)
                .Where(m => m.Id == id)
                .Select(m => new MajorResponse
                {
                    Id = m.Id,
                    FacultyId = m.FacultyId,
                    FacultyName = m.Faculty.Name,
                    Name = m.Name,
                    Code = m.Code,
                    AlumniCount = m.AlumniProfiles.Count
                })
                .FirstOrDefaultAsync();

            if (major == null)
                return NotFound(new { Message = "Ngành không tồn tại" });

            return Ok(major);
        }

        // POST: /api/majors (ADMIN)
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<MajorResponse>> CreateMajor(
            [FromBody] MajorCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check faculty tồn tại
            var facultyExists = await _context.Faculties
                .AnyAsync(f => f.Id == request.FacultyId);

            if (!facultyExists)
                return BadRequest(new { Message = "Khoa không tồn tại" });

            // Check trùng tên trong cùng khoa
            var existed = await _context.Majors
                .AnyAsync(m => m.FacultyId == request.FacultyId && m.Name == request.Name);

            if (existed)
                return Conflict(new { Message = "Ngành đã tồn tại trong khoa này" });

            var major = new Major
            {
                FacultyId = request.FacultyId,
                Name = request.Name,
                Code = request.Code
            };

            _context.Majors.Add(major);
            await _context.SaveChangesAsync();

            // load lại faculty để map response
            await _context.Entry(major).Reference(m => m.Faculty).LoadAsync();

            var response = new MajorResponse
            {
                Id = major.Id,
                FacultyId = major.FacultyId,
                FacultyName = major.Faculty.Name,
                Name = major.Name,
                Code = major.Code,
                AlumniCount = 0
            };

            return CreatedAtAction(nameof(GetMajor), new { id = major.Id }, response);
        }

        // PUT: /api/majors/{id} (ADMIN)
        [Authorize(Roles = "admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<MajorResponse>> UpdateMajor(
            int id,
            [FromBody] MajorUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var major = await _context.Majors
                .Include(m => m.Faculty)
                .Include(m => m.AlumniProfiles)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (major == null)
                return NotFound(new { Message = "Ngành không tồn tại" });

            // Check faculty tồn tại
            var facultyExists = await _context.Faculties
                .AnyAsync(f => f.Id == request.FacultyId);

            if (!facultyExists)
                return BadRequest(new { Message = "Khoa không tồn tại" });

            // Check trùng tên trong cùng khoa (trừ chính nó)
            var nameExisted = await _context.Majors
                .AnyAsync(m =>
                    m.Id != id &&
                    m.FacultyId == request.FacultyId &&
                    m.Name == request.Name);

            if (nameExisted)
                return Conflict(new { Message = "Ngành đã tồn tại trong khoa này" });

            major.FacultyId = request.FacultyId;
            major.Name = request.Name;
            major.Code = request.Code;

            await _context.SaveChangesAsync();

            // load lại faculty nếu đổi khoa
            await _context.Entry(major).Reference(m => m.Faculty).LoadAsync();

            var response = new MajorResponse
            {
                Id = major.Id,
                FacultyId = major.FacultyId,
                FacultyName = major.Faculty.Name,
                Name = major.Name,
                Code = major.Code,
                AlumniCount = major.AlumniProfiles.Count
            };

            return Ok(response);
        }

        // DELETE: /api/majors/{id} (ADMIN)
        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMajor(int id)
        {
            var major = await _context.Majors
                .Include(m => m.AlumniProfiles)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (major == null)
                return NotFound(new { Message = "Ngành không tồn tại" });

            if (major.AlumniProfiles.Any())
                return BadRequest(new { Message = "Không thể xóa ngành đang có hồ sơ cựu sinh viên" });

            _context.Majors.Remove(major);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Xóa ngành thành công" });
        }
    }
}
