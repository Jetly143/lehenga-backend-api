using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rent_Lehenga_Project.Data;
using Rent_Lehenga_Project.DTOs;
using Rent_Lehenga_Project.Models;

namespace Rent_Lehenga_Project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public BookingsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetBookings([FromQuery] long? userId, [FromQuery] string? status)
    {
        var query = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Listing)
            .Include(b => b.Payments)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(b => b.UserId == userId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(b => b.Status == status.ToUpper());

        var bookings = await query.ToListAsync();
        return Ok(bookings);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookingById(long id)
    {
        var booking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Listing)
            .Include(b => b.Payments)
            .Include(b => b.LogisticsTasks)
            .Include(b => b.TailorTasks)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
            return NotFound(new { message = "Booking not found." });

        return Ok(booking);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
            return BadRequest(new { message = "User not found." });

        var listing = await _context.Listings.FindAsync(dto.ListingId);
        if (listing == null)
            return BadRequest(new { message = "Listing not found." });

        var booking = new Booking
        {
            UserId = dto.UserId,
            ListingId = dto.ListingId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            SecurityDepositAmount = dto.SecurityDepositAmount,
            TotalAmount = dto.TotalAmount,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, booking);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateBookingStatusDto dto)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
            return NotFound(new { message = "Booking not found." });

        booking.Status = dto.Status.ToUpper();
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(booking);
    }
}
