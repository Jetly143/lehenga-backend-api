using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rent_Lehenga_Project.Data;
using Rent_Lehenga_Project.DTOs;
using Rent_Lehenga_Project.Models;

namespace Rent_Lehenga_Project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OperationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public OperationsController(AppDbContext context)
    {
        _context = context;
    }

    // --- LOGISTICS TASKS ---
    [HttpGet("logistics-tasks")]
    public async Task<IActionResult> GetLogisticsTasks([FromQuery] long? assignedTo)
    {
        var query = _context.LogisticsTasks
            .Include(lt => lt.Booking)
            .Include(lt => lt.Photos)
            .AsQueryable();

        if (assignedTo.HasValue)
            query = query.Where(lt => lt.AssignedTo == assignedTo.Value);

        return Ok(await query.ToListAsync());
    }

    [HttpPost("logistics-tasks")]
    public async Task<IActionResult> CreateLogisticsTask([FromBody] CreateLogisticsTaskDto dto)
    {
        var task = new LogisticsTask
        {
            BookingId = dto.BookingId,
            AssignedTo = dto.AssignedTo,
            Status = LogisticsTaskStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.LogisticsTasks.Add(task);
        await _context.SaveChangesAsync();

        return Ok(task);
    }

    [HttpPost("logistics-tasks/photos")]
    public async Task<IActionResult> AddTaskPhoto([FromBody] AddTaskPhotoDto dto)
    {
        var task = await _context.LogisticsTasks.FindAsync(dto.TaskId);
        if (task == null)
            return NotFound(new { message = "Logistics task not found." });

        var photo = new LogisticsTaskPhoto
        {
            TaskId = dto.TaskId,
            PhotoUrl = dto.PhotoUrl,
            Status = LogisticsTaskStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };

        _context.LogisticsTaskPhotos.Add(photo);
        await _context.SaveChangesAsync();

        return Ok(photo);
    }

    // --- TAILOR TASKS ---
    [HttpGet("tailor-tasks")]
    public async Task<IActionResult> GetTailorTasks([FromQuery] long? tailorId)
    {
        var query = _context.TailorTasks
            .Include(tt => tt.Booking)
            .AsQueryable();

        if (tailorId.HasValue)
            query = query.Where(tt => tt.TailorId == tailorId.Value);

        return Ok(await query.ToListAsync());
    }

    [HttpPost("tailor-tasks")]
    public async Task<IActionResult> CreateTailorTask([FromBody] CreateTailorTaskDto dto)
    {
        var task = new TailorTask
        {
            BookingId = dto.BookingId,
            TailorId = dto.TailorId,
            Status = TailorTaskStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TailorTasks.Add(task);
        await _context.SaveChangesAsync();

        return Ok(task);
    }

    // --- PAYOUT BATCHES ---
    [HttpGet("payout-batches")]
    public async Task<IActionResult> GetPayoutBatches([FromQuery] long? ownerId)
    {
        var query = _context.PayoutBatches
            .Include(pb => pb.Items)
            .AsQueryable();

        if (ownerId.HasValue)
            query = query.Where(pb => pb.OwnerId == ownerId.Value);

        return Ok(await query.ToListAsync());
    }
}
