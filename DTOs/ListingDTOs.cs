using System.ComponentModel.DataAnnotations;

namespace Rent_Lehenga_Project.DTOs;

public class CreateListingDto
{
    public long OwnerId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public string SupplyType { get; set; } = "ADMIN_OWN"; // ADMIN_OWN, B2B_SHOP, P2P_INDIVIDUAL

    public string Category { get; set; } = "BRIDAL"; // BRIDAL, SANGEET, RECEPTION, MEHENDI, PARTY

    public string? Fabric { get; set; }

    public string? Size { get; set; } = "M"; // XS, S, M, L, XL, FREE_SIZE

    public decimal? BustSize { get; set; }

    public decimal? WaistSize { get; set; }

    public decimal? SkirtLength { get; set; }

    [Required]
    [Range(1, 1000000)]
    public decimal PricePerDay { get; set; }

    public decimal SecurityDeposit { get; set; } = 0.00m;

    public decimal OriginalPrice { get; set; } = 0.00m;

    public int StockQty { get; set; } = 1;

    public List<ListingPhotoDto> Photos { get; set; } = new List<ListingPhotoDto>();
}

public class ListingPhotoDto
{
    public string PhotoUrl { get; set; } = string.Empty;
    public bool IsCover { get; set; } = false;
    public string PhotoType { get; set; } = "FRONT"; // FRONT, BACK, DETAIL, VIDEO
}

public class ApproveListingDto
{
    public string? AdminNotes { get; set; }
}

public class RejectListingDto
{
    [Required]
    public string RejectionReason { get; set; } = string.Empty;

    public string? AdminNotes { get; set; }
}

public class UpdateListingStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public class ListingResponseDto
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string? OwnerEmail { get; set; }
    public string? OwnerShopName { get; set; }
    public string? OwnerRole { get; set; }
    public string? OwnerCity { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SupplyType { get; set; } = "ADMIN_OWN";
    public string Category { get; set; } = "BRIDAL";
    public string? Fabric { get; set; }
    public string? Size { get; set; }
    public decimal? BustSize { get; set; }
    public decimal? WaistSize { get; set; }
    public decimal? SkirtLength { get; set; }

    public decimal PricePerDay { get; set; }
    public decimal SecurityDeposit { get; set; }
    public decimal OriginalPrice { get; set; }
    public int StockQty { get; set; }
    public string Status { get; set; } = "PENDING_VERIFICATION";

    public string? RejectionReason { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ListingPhotoDto> Photos { get; set; } = new List<ListingPhotoDto>();
}
