using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rent_Lehenga_Project.Data;
using Rent_Lehenga_Project.DTOs;
using Rent_Lehenga_Project.Models;

namespace Rent_Lehenga_Project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ListingsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 1. Get Listings with filters (Customer Catalog, Admin Filter, Occasion, Supply Type)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetListings(
        [FromQuery] string? status,
        [FromQuery] string? supplyType,
        [FromQuery] string? category,
        [FromQuery] string? search)
    {
        var query = _context.Listings
            .Include(l => l.Owner)
            .Include(l => l.Photos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.ToUpper();
            query = query.Where(l => l.Status == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(supplyType))
        {
            var normalizedSupply = supplyType.ToUpper();
            query = query.Where(l => l.SupplyType == normalizedSupply);
        }

        if (!string.IsNullOrWhiteSpace(category) && category.ToUpper() != "ALL")
        {
            var normalizedCategory = category.ToUpper();
            query = query.Where(l => l.Category == normalizedCategory);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLower();
            query = query.Where(l =>
                l.Title.ToLower().Contains(lowerSearch) ||
                (l.Fabric != null && l.Fabric.ToLower().Contains(lowerSearch)) ||
                l.Category.ToLower().Contains(lowerSearch));
        }

        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => MapToDto(l))
            .ToListAsync();

        return Ok(listings);
    }

    /// <summary>
    /// 2. Quality Verification Queue (Listings pending admin review)
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingListings()
    {
        var listings = await _context.Listings
            .Include(l => l.Owner)
            .Include(l => l.Photos)
            .Where(l => l.Status == ListingStatus.PendingVerification)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => MapToDto(l))
            .ToListAsync();

        return Ok(listings);
    }

    /// <summary>
    /// 3. Get listings by Owner ID (for P2P Lady or Boutique Partner portal)
    /// </summary>
    [HttpGet("owner/{ownerId}")]
    public async Task<IActionResult> GetListingsByOwner(long ownerId)
    {
        var listings = await _context.Listings
            .Include(l => l.Owner)
            .Include(l => l.Photos)
            .Where(l => l.OwnerId == ownerId)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => MapToDto(l))
            .ToListAsync();

        return Ok(listings);
    }

    /// <summary>
    /// 4. Get Single Listing Details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetListingById(long id)
    {
        var listing = await _context.Listings
            .Include(l => l.Owner)
            .Include(l => l.Photos)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (listing == null)
            return NotFound(new { message = "Listing not found." });

        return Ok(MapToDto(listing));
    }

    /// <summary>
    /// 5. Create New Listing with Photos (Admin In-House, B2B Shop, or P2P Lady)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateListing([FromBody] CreateListingDto dto)
    {
        var owner = await _context.Users.FindAsync(dto.OwnerId);
        if (owner == null)
            return BadRequest(new { message = "Owner account not found." });

        var normalizedSupply = (dto.SupplyType ?? SupplyType.AdminOwn).ToUpper();
        var normalizedCategory = (dto.Category ?? "BRIDAL").ToUpper();

        // Admin-owned inventory is auto-verified, 3rd party listings go to Pending Verification
        var initialStatus = (owner.Role == UserRole.Admin)
            ? ListingStatus.Active
            : ListingStatus.PendingVerification;

        var listing = new Listing
        {
            OwnerId = dto.OwnerId,
            Title = dto.Title,
            Description = dto.Description,
            SupplyType = normalizedSupply,
            Category = normalizedCategory,
            Fabric = dto.Fabric,
            Size = dto.Size ?? "M",
            BustSize = dto.BustSize,
            WaistSize = dto.WaistSize,
            SkirtLength = dto.SkirtLength,
            PricePerDay = dto.PricePerDay,
            SecurityDeposit = dto.SecurityDeposit > 0 ? dto.SecurityDeposit : dto.PricePerDay * 2.5m,
            OriginalPrice = dto.OriginalPrice,
            StockQty = dto.StockQty > 0 ? dto.StockQty : 1,
            Status = initialStatus,
            VerifiedAt = (initialStatus == ListingStatus.Active) ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Listings.Add(listing);
        await _context.SaveChangesAsync();

        // Save Photos
        if (dto.Photos != null && dto.Photos.Any())
        {
            foreach (var photoDto in dto.Photos)
            {
                if (!string.IsNullOrWhiteSpace(photoDto.PhotoUrl))
                {
                    _context.ListingPhotos.Add(new ListingPhoto
                    {
                        ListingId = listing.Id,
                        PhotoUrl = photoDto.PhotoUrl,
                        IsCover = photoDto.IsCover,
                        PhotoType = photoDto.PhotoType ?? "FRONT",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        // Re-fetch with includes for response
        var created = await _context.Listings
            .Include(l => l.Owner)
            .Include(l => l.Photos)
            .FirstAsync(l => l.Id == listing.Id);

        return CreatedAtAction(nameof(GetListingById), new { id = listing.Id }, MapToDto(created));
    }

    /// <summary>
    /// 6. Quality Verification: Approve Listing
    /// </summary>
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApproveListing(long id, [FromBody] ApproveListingDto dto)
    {
        var listing = await _context.Listings
            .Include(l => l.Owner)
            .Include(l => l.Photos)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (listing == null)
            return NotFound(new { message = "Listing not found." });

        listing.Status = ListingStatus.Active;
        listing.AdminNotes = dto.AdminNotes ?? "Approved for rental catalog";
        listing.RejectionReason = null;
        listing.VerifiedAt = DateTime.UtcNow;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new
        {
            message = "Listing approved successfully. It is now live in the rental catalog.",
            listing = MapToDto(listing)
        });
    }

    /// <summary>
    /// 7. Quality Verification: Reject Listing
    /// </summary>
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> RejectListing(long id, [FromBody] RejectListingDto dto)
    {
        var listing = await _context.Listings
            .Include(l => l.Owner)
            .Include(l => l.Photos)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (listing == null)
            return NotFound(new { message = "Listing not found." });

        listing.Status = ListingStatus.Rejected;
        listing.RejectionReason = dto.RejectionReason;
        listing.AdminNotes = dto.AdminNotes;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new
        {
            message = "Listing rejected. Feedback sent to owner.",
            listing = MapToDto(listing)
        });
    }

    /// <summary>
    /// 8. Delete Listing
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteListing(long id)
    {
        var listing = await _context.Listings.FindAsync(id);
        if (listing == null)
            return NotFound(new { message = "Listing not found." });

        _context.Listings.Remove(listing);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Listing deleted successfully." });
    }

    // Helper to map entity to response DTO
    private static ListingResponseDto MapToDto(Listing l)
    {
        return new ListingResponseDto
        {
            Id = l.Id,
            OwnerId = l.OwnerId,
            OwnerName = l.Owner?.Name ?? "LehengaVerse",
            OwnerEmail = l.Owner?.Email,
            OwnerShopName = l.Owner?.ShopName,
            OwnerRole = l.Owner?.Role,
            OwnerCity = l.Owner?.City ?? "Delhi",
            Title = l.Title,
            Description = l.Description,
            SupplyType = l.SupplyType,
            Category = l.Category,
            Fabric = l.Fabric,
            Size = l.Size,
            BustSize = l.BustSize,
            WaistSize = l.WaistSize,
            SkirtLength = l.SkirtLength,
            PricePerDay = l.PricePerDay,
            SecurityDeposit = l.SecurityDeposit,
            OriginalPrice = l.OriginalPrice,
            StockQty = l.StockQty,
            Status = l.Status,
            RejectionReason = l.RejectionReason,
            AdminNotes = l.AdminNotes,
            VerifiedAt = l.VerifiedAt,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt,
            Photos = l.Photos.Select(p => new ListingPhotoDto
            {
                PhotoUrl = p.PhotoUrl,
                IsCover = p.IsCover,
                PhotoType = p.PhotoType
            }).ToList()
        };
    }
}
