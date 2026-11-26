using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rent_Lehenga_Project.Data;
using Rent_Lehenga_Project.DTOs;
using Rent_Lehenga_Project.Models;

namespace Rent_Lehenga_Project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PaymentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPayments([FromQuery] long? bookingId)
    {
        var query = _context.Payments.AsQueryable();
        if (bookingId.HasValue)
            query = query.Where(p => p.BookingId == bookingId.Value);

        var payments = await query.ToListAsync();
        return Ok(payments);
    }

    [HttpPost]
    public async Task<IActionResult> RecordPayment([FromBody] CreatePaymentDto dto)
    {
        var booking = await _context.Bookings.FindAsync(dto.BookingId);
        if (booking == null)
            return BadRequest(new { message = "Booking not found." });

        var payment = new Payment
        {
            BookingId = dto.BookingId,
            Amount = dto.Amount,
            PaymentStatus = dto.PaymentStatus.ToUpper(),
            TransactionId = string.IsNullOrEmpty(dto.TransactionId) 
                ? $"TXN_{Guid.NewGuid():N}".Substring(0, 20).ToUpper() 
                : dto.TransactionId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);

        if (payment.PaymentStatus == PaymentStatus.Success)
        {
            booking.Status = BookingStatus.Confirmed;
            booking.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(payment);
    }
}
