using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCSV.Data;
using QLCSV.DTOs.Faculty;
using QLCSV.Models;

namespace QLCSV.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacultiesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FacultiesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/faculties
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FacultyResponse>>> GetFaculties()
        {
            var faculties = await _context.Faculties
                .Include(f => f.Majors)
                .Select(f => new FacultyResponse
                {
                    Id = f.Id,
                    Name = f.Name,
                    ShortName = f.ShortName,
                    Description = f.Description,
                    MajorCount = f.Majors.Count
                })
                .ToListAsync();

            return Ok(faculties);
        }

        // GET: /api/faculties/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FacultyResponse>> GetFaculty(int id)
        {
            var faculty = await _context.Faculties
                .Include(f => f.Majors)
                .Select(f => new FacultyResponse
                {
                    Id = f.Id,
                    Name = f.Name,
                    ShortName = f.ShortName,
                    Description = f.Description,
                    MajorCount = f.Majors.Count
                })
                .FirstOrDefaultAsync(f => f.Id == id);

            if (faculty == null)
                return NotFound(new { Message = "Khoa không tồn tại" });

            return Ok(faculty);
        }

        // POST: /api/faculties  (ADMIN)
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<FacultyResponse>> CreateFaculty([FromBody] FacultyCreateRequest request)
        {
            // [ApiController] tự validate ModelState dựa vào annotation
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existed = await _context.Faculties
                .AnyAsync(f => f.Name == request.Name);

            if (existed)
                return Conflict(new { Message = "Tên khoa đã tồn tại" });

            var faculty = new Faculty
            {
                Name = request.Name,
                ShortName = request.ShortName,
                Description = request.Description
            };

            _context.Faculties.Add(faculty);
            await _context.SaveChangesAsync();

            var response = new FacultyResponse
            {
                Id = faculty.Id,
                Name = faculty.Name,
                ShortName = faculty.ShortName,
                Description = faculty.Description,
                MajorCount = 0
            };

            return CreatedAtAction(nameof(GetFaculty), new { id = faculty.Id }, response);
        }

        // PUT: /api/faculties/{id}  (ADMIN)
        [Authorize(Roles = "admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<FacultyResponse>> UpdateFaculty(
            int id,
            [FromBody] FacultyUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var faculty = await _context.Faculties
                .Include(f => f.Majors)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (faculty == null)
                return NotFound(new { Message = "Khoa không tồn tại" });

            // check trùng tên (trừ chính nó)
            var nameExisted = await _context.Faculties
                .AnyAsync(f => f.Id != id && f.Name == request.Name);

            if (nameExisted)
                return Conflict(new { Message = "Tên khoa đã tồn tại" });

            faculty.Name = request.Name;
            faculty.ShortName = request.ShortName;
            faculty.Description = request.Description;

            await _context.SaveChangesAsync();

            var response = new FacultyResponse
            {
                Id = faculty.Id,
                Name = faculty.Name,
                ShortName = faculty.ShortName,
                Description = faculty.Description,
                MajorCount = faculty.Majors.Count
            };

            return Ok(response);
        }

        // DELETE: /api/faculties/{id}  (ADMIN)
        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFaculty(int id)
        {
            var faculty = await _context.Faculties
                .Include(f => f.Majors)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (faculty == null)
                return NotFound(new { Message = "Khoa không tồn tại" });

            if (faculty.Majors.Any())
                return BadRequest(new { Message = "Không thể xóa khoa đang có ngành" });

            _context.Faculties.Remove(faculty);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Xóa khoa thành công" });
        }
    }
}
