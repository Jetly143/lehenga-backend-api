using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rent_Lehenga_Project.Models;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }

    [Required]
    [Column("role")]
    public string Role { get; set; } = UserRole.Customer;

    [Required]
    [MaxLength(255)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    // KYC & Business Profile Fields
    [Required]
    [MaxLength(50)]
    [Column("kyc_status")]
    public string KycStatus { get; set; } = Models.KycStatus.NotSubmitted;

    [MaxLength(50)]
    [Column("aadhar_number")]
    public string? AadharNumber { get; set; }

    [MaxLength(500)]
    [Column("aadhar_doc_url")]
    public string? AadharDocUrl { get; set; }

    [MaxLength(500)]
    [Column("selfie_video_url")]
    public string? SelfieVideoUrl { get; set; }

    [MaxLength(150)]
    [Column("shop_name")]
    public string? ShopName { get; set; }

    [MaxLength(50)]
    [Column("gst_number")]
    public string? GstNumber { get; set; }

    [MaxLength(255)]
    [Column("address")]
    public string? Address { get; set; }

    [MaxLength(100)]
    [Column("city")]
    public string? City { get; set; }

    [Column("is_verified")]
    public bool IsVerified { get; set; } = false;

    [Column("is_email_verified")]
    public bool IsEmailVerified { get; set; } = false;

    [MaxLength(255)]
    [Column("email_verification_token")]
    public string? EmailVerificationToken { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<PayoutBatch> PayoutBatches { get; set; } = new List<PayoutBatch>();
    public ICollection<LogisticsTask> AssignedLogisticsTasks { get; set; } = new List<LogisticsTask>();
    public ICollection<TailorTask> AssignedTailorTasks { get; set; } = new List<TailorTask>();
}

[Table("listings")]
public class Listing
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("owner_id")]
    public long OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public User? Owner { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    // 3 Supply Models: ADMIN_OWN, B2B_SHOP, P2P_INDIVIDUAL
    [Required]
    [MaxLength(50)]
    [Column("supply_type")]
    public string SupplyType { get; set; } = Models.SupplyType.AdminOwn;

    [MaxLength(50)]
    [Column("category")]
    public string Category { get; set; } = "BRIDAL"; // BRIDAL, SANGEET, RECEPTION, MEHENDI, PARTY

    [MaxLength(100)]
    [Column("fabric")]
    public string? Fabric { get; set; }

    [MaxLength(20)]
    [Column("size")]
    public string? Size { get; set; } // XS, S, M, L, XL, FREE_SIZE

    [Column("bust_size", TypeName = "decimal(5,2)")]
    public decimal? BustSize { get; set; }

    [Column("waist_size", TypeName = "decimal(5,2)")]
    public decimal? WaistSize { get; set; }

    [Column("skirt_length", TypeName = "decimal(5,2)")]
    public decimal? SkirtLength { get; set; }

    [Required]
    [Column("price_per_day", TypeName = "decimal(10,2)")]
    public decimal PricePerDay { get; set; }

    [Column("security_deposit", TypeName = "decimal(10,2)")]
    public decimal SecurityDeposit { get; set; } = 0.00m;

    [Column("original_price", TypeName = "decimal(10,2)")]
    public decimal OriginalPrice { get; set; } = 0.00m;

    [Column("stock_qty")]
    public int StockQty { get; set; } = 1;

    [Required]
    [Column("status")]
    public string Status { get; set; } = ListingStatus.PendingVerification;

    // Quality Inspection Fields
    [MaxLength(500)]
    [Column("rejection_reason")]
    public string? RejectionReason { get; set; }

    [MaxLength(500)]
    [Column("admin_notes")]
    public string? AdminNotes { get; set; }

    [Column("verified_at")]
    public DateTime? VerifiedAt { get; set; }

    [Column("verified_by_user_id")]
    public long? VerifiedByUserId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ListingPhoto> Photos { get; set; } = new List<ListingPhoto>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

[Table("listing_photos")]
public class ListingPhoto
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("listing_id")]
    public long ListingId { get; set; }

    [ForeignKey(nameof(ListingId))]
    public Listing? Listing { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("photo_url")]
    public string PhotoUrl { get; set; } = string.Empty;

    [Column("is_cover")]
    public bool IsCover { get; set; } = false;

    [MaxLength(50)]
    [Column("photo_type")]
    public string PhotoType { get; set; } = "FRONT"; // FRONT, BACK, DETAIL, VIDEO_360

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("bookings")]
public class Booking
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("user_id")]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    [Column("listing_id")]
    public long ListingId { get; set; }

    [ForeignKey(nameof(ListingId))]
    public Listing? Listing { get; set; }

    [Required]
    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Required]
    [Column("end_date")]
    public DateOnly EndDate { get; set; }

    [Required]
    [Column("status")]
    public string Status { get; set; } = BookingStatus.Pending;

    [Column("security_deposit_amount", TypeName = "decimal(10,2)")]
    public decimal SecurityDepositAmount { get; set; } = 0.00m;

    [Required]
    [Column("total_amount", TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public PayoutItem? PayoutItem { get; set; }
    public ICollection<LogisticsTask> LogisticsTasks { get; set; } = new List<LogisticsTask>();
    public ICollection<TailorTask> TailorTasks { get; set; } = new List<TailorTask>();
}

[Table("payments")]
public class Payment
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("booking_id")]
    public long BookingId { get; set; }

    [ForeignKey(nameof(BookingId))]
    public Booking? Booking { get; set; }

    [Required]
    [Column("amount", TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [Required]
    [Column("payment_status")]
    public string PaymentStatus { get; set; } = Models.PaymentStatus.Pending;

    [MaxLength(100)]
    [Column("transaction_id")]
    public string? TransactionId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("notifications")]
public class Notification
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("user_id")]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("type")]
    public string Type { get; set; } = string.Empty;

    [Required]
    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("payout_batches")]
public class PayoutBatch
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("owner_id")]
    public long OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public User? Owner { get; set; }

    [Required]
    [Column("batch_date")]
    public DateOnly BatchDate { get; set; }

    [Column("total_amount", TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; } = 0.00m;

    [Required]
    [Column("status")]
    public string Status { get; set; } = PayoutBatchStatus.Pending;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public ICollection<PayoutItem> Items { get; set; } = new List<PayoutItem>();
}

[Table("payout_items")]
public class PayoutItem
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("payout_batch_id")]
    public long PayoutBatchId { get; set; }

    [ForeignKey(nameof(PayoutBatchId))]
    public PayoutBatch? PayoutBatch { get; set; }

    [Required]
    [Column("booking_id")]
    public long BookingId { get; set; }

    [ForeignKey(nameof(BookingId))]
    public Booking? Booking { get; set; }

    [Required]
    [Column("amount", TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("logistics_tasks")]
public class LogisticsTask
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("booking_id")]
    public long BookingId { get; set; }

    [ForeignKey(nameof(BookingId))]
    public Booking? Booking { get; set; }

    [Required]
    [Column("assigned_to")]
    public long AssignedTo { get; set; }

    [ForeignKey(nameof(AssignedTo))]
    public User? AssignedUser { get; set; }

    [Required]
    [Column("status")]
    public string Status { get; set; } = LogisticsTaskStatus.Pending;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<LogisticsTaskPhoto> Photos { get; set; } = new List<LogisticsTaskPhoto>();
}

[Table("logistics_task_photos")]
public class LogisticsTaskPhoto
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("task_id")]
    public long TaskId { get; set; }

    [ForeignKey(nameof(TaskId))]
    public LogisticsTask? Task { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("photo_url")]
    public string PhotoUrl { get; set; } = string.Empty;

    [Required]
    [Column("status")]
    public string Status { get; set; } = LogisticsTaskStatus.Pending;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("tailor_tasks")]
public class TailorTask
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("booking_id")]
    public long BookingId { get; set; }

    [ForeignKey(nameof(BookingId))]
    public Booking? Booking { get; set; }

    [Required]
    [Column("tailor_id")]
    public long TailorId { get; set; }

    [ForeignKey(nameof(TailorId))]
    public User? Tailor { get; set; }

    [Required]
    [Column("status")]
    public string Status { get; set; } = TailorTaskStatus.Pending;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
