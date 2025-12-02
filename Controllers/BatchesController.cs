using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCSV.Data;
using QLCSV.DTOs.Batch;
using QLCSV.Models;

namespace QLCSV.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BatchesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BatchesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/batches?graduationYear=2020
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BatchResponse>>> GetBatches(
            [FromQuery] int? graduationYear)
        {
            var query = _context.Batches
                .Include(b => b.AlumniBatches)
                .AsQueryable();

            if (graduationYear.HasValue)
            {
                query = query.Where(b => b.GraduationYear == graduationYear.Value);
            }

            var batches = await query
                .Select(b => new BatchResponse
                {
                    Id = b.Id,
                    GraduationYear = b.GraduationYear,
                    Name = b.Name,
                    StartYear = b.StartYear,
                    Description = b.Description,
                    AlumniCount = b.AlumniBatches.Count
                })
                .ToListAsync();

            return Ok(batches);
        }

        // GET: /api/batches/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BatchResponse>> GetBatch(int id)
        {
            var batch = await _context.Batches
                .Include(b => b.AlumniBatches)
                .Where(b => b.Id == id)
                .Select(b => new BatchResponse
                {
                    Id = b.Id,
                    GraduationYear = b.GraduationYear,
                    Name = b.Name,
                    StartYear = b.StartYear,
                    Description = b.Description,
                    AlumniCount = b.AlumniBatches.Count
                })
                .FirstOrDefaultAsync();

            if (batch == null)
                return NotFound(new { Message = "Khóa không tồn tại" });

            return Ok(batch);
        }

        // POST: /api/batches (ADMIN)
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<BatchResponse>> CreateBatch(
            [FromBody] BatchCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check trùng tên + năm tốt nghiệp
            var existed = await _context.Batches
                .AnyAsync(b =>
                    b.GraduationYear == request.GraduationYear &&
                    b.Name == request.Name);

            if (existed)
                return Conflict(new { Message = "Khóa với tên và năm tốt nghiệp này đã tồn tại" });

            var batch = new Batch
            {
                GraduationYear = request.GraduationYear,
                Name = request.Name,
                StartYear = request.StartYear,
                Description = request.Description
            };

            _context.Batches.Add(batch);
            await _context.SaveChangesAsync();

            var response = new BatchResponse
            {
                Id = batch.Id,
                GraduationYear = batch.GraduationYear,
                Name = batch.Name,
                StartYear = batch.StartYear,
                Description = batch.Description,
                AlumniCount = 0
            };

            return CreatedAtAction(nameof(GetBatch), new { id = batch.Id }, response);
        }

        // PUT: /api/batches/{id} (ADMIN)
        [Authorize(Roles = "admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<BatchResponse>> UpdateBatch(
            int id,
            [FromBody] BatchUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var batch = await _context.Batches
                .Include(b => b.AlumniBatches)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (batch == null)
                return NotFound(new { Message = "Khóa không tồn tại" });

            // Check trùng tên + năm tốt nghiệp (trừ chính nó)
            var existed = await _context.Batches
                .AnyAsync(b =>
                    b.Id != id &&
                    b.GraduationYear == request.GraduationYear &&
                    b.Name == request.Name);

            if (existed)
                return Conflict(new { Message = "Khóa với tên và năm tốt nghiệp này đã tồn tại" });

            batch.GraduationYear = request.GraduationYear;
            batch.Name = request.Name;
            batch.StartYear = request.StartYear;
            batch.Description = request.Description;

            await _context.SaveChangesAsync();

            var response = new BatchResponse
            {
                Id = batch.Id,
                GraduationYear = batch.GraduationYear,
                Name = batch.Name,
                StartYear = batch.StartYear,
                Description = batch.Description,
                AlumniCount = batch.AlumniBatches.Count
            };

            return Ok(response);
        }

        // DELETE: /api/batches/{id} (ADMIN)
        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBatch(int id)
        {
            var batch = await _context.Batches
                .Include(b => b.AlumniBatches)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (batch == null)
                return NotFound(new { Message = "Khóa không tồn tại" });

            if (batch.AlumniBatches.Any())
                return BadRequest(new { Message = "Không thể xóa khóa đang có cựu sinh viên" });

            _context.Batches.Remove(batch);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Xóa khóa thành công" });
        }
    }
}
