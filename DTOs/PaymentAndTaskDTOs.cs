namespace Rent_Lehenga_Project.DTOs;

public class CreatePaymentDto
{
    public long BookingId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = "SUCCESS"; // PENDING, SUCCESS, FAILED, REFUNDED
    public string? TransactionId { get; set; }
}

public class CreateLogisticsTaskDto
{
    public long BookingId { get; set; }
    public long AssignedTo { get; set; }
}

public class AddTaskPhotoDto
{
    public long TaskId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
}

public class CreateTailorTaskDto
{
    public long BookingId { get; set; }
    public long TailorId { get; set; }
}
