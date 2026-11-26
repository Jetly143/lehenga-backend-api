namespace Rent_Lehenga_Project.DTOs;

public class CreateBookingDto
{
    public long UserId { get; set; }
    public long ListingId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal SecurityDepositAmount { get; set; } = 0.00m;
    public decimal TotalAmount { get; set; }
}

public class UpdateBookingStatusDto
{
    public string Status { get; set; } = string.Empty; // PENDING, CONFIRMED, ALTERATION, READY_TO_SHIP, DELIVERED, RETURNED, CANCELLED, COMPLETED
}
