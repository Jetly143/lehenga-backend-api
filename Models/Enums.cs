namespace Rent_Lehenga_Project.Models;

public static class UserRole
{
    public const string Customer = "CUSTOMER";
    public const string IndividualOwner = "INDIVIDUAL_OWNER";
    public const string ShopOwner = "SHOP_OWNER";
    public const string Owner = "OWNER"; // Backward compatibility
    public const string DeliveryAgent = "DELIVERY_AGENT";
    public const string Tailor = "TAILOR";
    public const string Admin = "ADMIN";
}

public static class SupplyType
{
    public const string AdminOwn = "ADMIN_OWN";
    public const string B2BShop = "B2B_SHOP";
    public const string P2PIndividual = "P2P_INDIVIDUAL";
}

public static class KycStatus
{
    public const string NotSubmitted = "NOT_SUBMITTED";
    public const string Pending = "PENDING";
    public const string Verified = "VERIFIED";
    public const string Rejected = "REJECTED";
}

public static class ListingStatus
{
    public const string PendingVerification = "PENDING_VERIFICATION";
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";
    public const string Rejected = "REJECTED";
    public const string UnderAlteration = "UNDER_ALTERATION";
}

public static class BookingStatus
{
    public const string Pending = "PENDING";
    public const string Confirmed = "CONFIRMED";
    public const string Alteration = "ALTERATION";
    public const string ReadyToShip = "READY_TO_SHIP";
    public const string Delivered = "DELIVERED";
    public const string Returned = "RETURNED";
    public const string Cancelled = "CANCELLED";
    public const string Completed = "COMPLETED";
}

public static class PaymentStatus
{
    public const string Pending = "PENDING";
    public const string Success = "SUCCESS";
    public const string Failed = "FAILED";
    public const string Refunded = "REFUNDED";
}

public static class PayoutBatchStatus
{
    public const string Pending = "PENDING";
    public const string Processing = "PROCESSING";
    public const string Paid = "PAID";
    public const string Failed = "FAILED";
}

public static class LogisticsTaskStatus
{
    public const string Pending = "PENDING";
    public const string InProgress = "IN_PROGRESS";
    public const string Completed = "COMPLETED";
    public const string Failed = "FAILED";
}

public static class TailorTaskStatus
{
    public const string Pending = "PENDING";
    public const string InProgress = "IN_PROGRESS";
    public const string Completed = "COMPLETED";
    public const string Cancelled = "CANCELLED";
}
