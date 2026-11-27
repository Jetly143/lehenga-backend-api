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

    // --- SEED BUSINESS MODEL DEMO DATA ---
    [HttpPost("seed-demo-data")]
    public async Task<IActionResult> SeedDemoData()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Admin@12345");

        // 1. Ensure or create Admin
        var admin = await _context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Admin);
        if (admin == null)
        {
            admin = new User
            {
                Name = "Gourav Kumar",
                Email = "admin@lehengaverse.com",
                PasswordHash = hash,
                Phone = "9999999999",
                Role = UserRole.Admin,
                KycStatus = KycStatus.Verified,
                IsVerified = true,
                IsEmailVerified = true,
                City = "Delhi",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(admin);
            await _context.SaveChangesAsync();
        }

        // 2. B2B Shop Partner 1 (Verified)
        var b2bShop = await _context.Users.FirstOrDefaultAsync(u => u.Email == "rajesh@royalbanaras.com");
        if (b2bShop == null)
        {
            b2bShop = new User
            {
                Name = "Rajesh Singhania",
                Email = "rajesh@royalbanaras.com",
                PasswordHash = hash,
                Phone = "9876543210",
                Role = UserRole.ShopOwner,
                ShopName = "Royal Banaras Studio",
                GstNumber = "09AAECR1234F1Z8",
                Address = "D-48/12 Godowlia Chowk",
                City = "Varanasi",
                KycStatus = KycStatus.Verified,
                IsVerified = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(b2bShop);
            await _context.SaveChangesAsync();
        }

        // 3. B2B Shop Partner 2 (Pending KYC)
        var b2bPending = await _context.Users.FirstOrDefaultAsync(u => u.Email == "anita@chandniboutique.com");
        if (b2bPending == null)
        {
            b2bPending = new User
            {
                Name = "Anita Sharma",
                Email = "anita@chandniboutique.com",
                PasswordHash = hash,
                Phone = "9811223344",
                Role = UserRole.ShopOwner,
                ShopName = "Chandni Boutique",
                GstNumber = "07BBECS5678G2Z1",
                Address = "Shop 14, Main Market, Chandni Chowk",
                City = "Delhi",
                KycStatus = KycStatus.Pending,
                IsVerified = false,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(b2bPending);
            await _context.SaveChangesAsync();
        }

        // 4. P2P Individual Partner 1 (Verified)
        var p2pVerified = await _context.Users.FirstOrDefaultAsync(u => u.Email == "pooja.verma@gmail.com");
        if (p2pVerified == null)
        {
            p2pVerified = new User
            {
                Name = "Pooja Verma",
                Email = "pooja.verma@gmail.com",
                PasswordHash = hash,
                Phone = "9712345678",
                Role = UserRole.IndividualOwner,
                AadharNumber = "XXXX-XXXX-4892",
                AadharDocUrl = "https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=600",
                SelfieVideoUrl = "https://assets.mixkit.co/videos/preview/mixkit-girl-smiling-portrait-3829-large.mp4",
                Address = "Plot 24, Malviya Nagar",
                City = "Jaipur",
                KycStatus = KycStatus.Verified,
                IsVerified = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(p2pVerified);
            await _context.SaveChangesAsync();
        }

        // 5. P2P Individual Partner 2 (Pending KYC)
        var p2pPending = await _context.Users.FirstOrDefaultAsync(u => u.Email == "meenakshi.iyer@gmail.com");
        if (p2pPending == null)
        {
            p2pPending = new User
            {
                Name = "Meenakshi Iyer",
                Email = "meenakshi.iyer@gmail.com",
                PasswordHash = hash,
                Phone = "9845012345",
                Role = UserRole.IndividualOwner,
                AadharNumber = "XXXX-XXXX-7123",
                AadharDocUrl = "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=600",
                Address = "12th Cross, Indiranagar",
                City = "Bengaluru",
                KycStatus = KycStatus.Pending,
                IsVerified = false,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(p2pPending);
            await _context.SaveChangesAsync();
        }

        // 6. Customers
        var customer1 = await _context.Users.FirstOrDefaultAsync(u => u.Email == "ananya.roy@gmail.com");
        if (customer1 == null)
        {
            customer1 = new User
            {
                Name = "Ananya Roy",
                Email = "ananya.roy@gmail.com",
                PasswordHash = hash,
                Phone = "9920112233",
                Role = UserRole.Customer,
                City = "Mumbai",
                IsVerified = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(customer1);
            await _context.SaveChangesAsync();
        }

        var customer2 = await _context.Users.FirstOrDefaultAsync(u => u.Email == "jyotiraut2002@gmail.com");
        if (customer2 == null)
        {
            customer2 = new User
            {
                Name = "Jyoti Raut",
                Email = "jyotiraut2002@gmail.com",
                PasswordHash = hash,
                Phone = "9667488857",
                Role = UserRole.Customer,
                City = "Delhi",
                IsVerified = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(customer2);
            await _context.SaveChangesAsync();
        }

        // 7. Tailor
        var tailor = await _context.Users.FirstOrDefaultAsync(u => u.Email == "master.ramesh@lehengaverse.com");
        if (tailor == null)
        {
            tailor = new User
            {
                Name = "Master Ramesh",
                Email = "master.ramesh@lehengaverse.com",
                PasswordHash = hash,
                Phone = "9810987654",
                Role = UserRole.Tailor,
                City = "Delhi",
                IsVerified = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(tailor);
            await _context.SaveChangesAsync();
        }

        // 8. Listings
        // Listing 1: B2B Royal Banarasi
        var listing1 = await _context.Listings.FirstOrDefaultAsync(l => l.Title.Contains("Banarasi Kadwa"));
        if (listing1 == null)
        {
            listing1 = new Listing
            {
                OwnerId = b2bShop.Id,
                Title = "Royal Banarasi Handloom Kadwa Zari Lehenga",
                Description = "Authentic heritage weave from Varanasi with intricate real gold zari border and rich raw silk blouse.",
                SupplyType = SupplyType.B2BShop,
                Category = "BRIDAL",
                Fabric = "Pure Banarasi Katan Silk",
                Size = "M",
                BustSize = 36.00m,
                WaistSize = 30.00m,
                SkirtLength = 42.00m,
                PricePerDay = 4200.00m,
                SecurityDeposit = 10000.00m,
                OriginalPrice = 85000.00m,
                StockQty = 1,
                Status = ListingStatus.Active,
                VerifiedAt = DateTime.UtcNow,
                VerifiedByUserId = admin.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Listings.Add(listing1);
            await _context.SaveChangesAsync();

            _context.ListingPhotos.AddRange(
                new ListingPhoto { ListingId = listing1.Id, PhotoUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=800", IsCover = true, PhotoType = "FRONT", CreatedAt = DateTime.UtcNow },
                new ListingPhoto { ListingId = listing1.Id, PhotoUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=800", IsCover = false, PhotoType = "DETAIL", CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();
        }

        // Listing 2: P2P Sabyasachi Crimson Velvet
        var listing2 = await _context.Listings.FirstOrDefaultAsync(l => l.Title.Contains("Sabyasachi Heritage Crimson"));
        if (listing2 == null)
        {
            listing2 = new Listing
            {
                OwnerId = p2pVerified.Id,
                Title = "Sabyasachi Heritage Crimson Velvet Bridal Lehenga",
                Description = "Purchased for personal wedding, worn once for 4 hours. Pristine condition with double dupatta and heavy zardozi.",
                SupplyType = SupplyType.P2PIndividual,
                Category = "BRIDAL",
                Fabric = "Micro Velvet with Dabka & Zardozi",
                Size = "S",
                BustSize = 34.00m,
                WaistSize = 28.00m,
                SkirtLength = 43.00m,
                PricePerDay = 5500.00m,
                SecurityDeposit = 15000.00m,
                OriginalPrice = 165000.00m,
                StockQty = 1,
                Status = ListingStatus.Active,
                VerifiedAt = DateTime.UtcNow,
                VerifiedByUserId = admin.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Listings.Add(listing2);
            await _context.SaveChangesAsync();

            _context.ListingPhotos.AddRange(
                new ListingPhoto { ListingId = listing2.Id, PhotoUrl = "https://images.unsplash.com/photo-1594744803329-e58b31de8bf5?w=800", IsCover = true, PhotoType = "FRONT", CreatedAt = DateTime.UtcNow },
                new ListingPhoto { ListingId = listing2.Id, PhotoUrl = "https://images.unsplash.com/photo-1617627143750-d86bc21e42bb?w=800", IsCover = false, PhotoType = "DETAIL", CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();
        }

        // Listing 3: ADMIN OWN Reception Lehenga
        var listing3 = await _context.Listings.FirstOrDefaultAsync(l => l.Title.Contains("Pastel Mint Green"));
        if (listing3 == null)
        {
            listing3 = new Listing
            {
                OwnerId = admin.Id,
                Title = "Pastel Mint Green Mirror Work Reception Lehenga",
                Description = "Curated luxury piece from LehengaVerse in-house runway collection. Intricate hand-cut mirror embroidery on soft organza.",
                SupplyType = SupplyType.AdminOwn,
                Category = "RECEPTION",
                Fabric = "Organza Silk with Real Mirror & Resham",
                Size = "FREE_SIZE",
                BustSize = 36.00m,
                WaistSize = 32.00m,
                SkirtLength = 42.00m,
                PricePerDay = 3500.00m,
                SecurityDeposit = 8000.00m,
                OriginalPrice = 55000.00m,
                StockQty = 2,
                Status = ListingStatus.Active,
                VerifiedAt = DateTime.UtcNow,
                VerifiedByUserId = admin.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Listings.Add(listing3);
            await _context.SaveChangesAsync();

            _context.ListingPhotos.AddRange(
                new ListingPhoto { ListingId = listing3.Id, PhotoUrl = "https://images.unsplash.com/photo-1610030469668-932d0c24b22c?w=800", IsCover = true, PhotoType = "FRONT", CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();
        }

        // Listing 4: P2P Pending Verification
        var listing4 = await _context.Listings.FirstOrDefaultAsync(l => l.Title.Contains("Marigold Yellow"));
        if (listing4 == null)
        {
            listing4 = new Listing
            {
                OwnerId = p2pPending.Id,
                Title = "Marigold Yellow Gota Patti Sangeet Lehenga",
                Description = "Bright festive lehenga ideal for haldi or sangeet night. Features pure georgette flare with silver gotta work.",
                SupplyType = SupplyType.P2PIndividual,
                Category = "SANGEET",
                Fabric = "Georgette with Handcrafted Gota Patti",
                Size = "M",
                BustSize = 38.00m,
                WaistSize = 32.00m,
                SkirtLength = 41.00m,
                PricePerDay = 2800.00m,
                SecurityDeposit = 6000.00m,
                OriginalPrice = 38000.00m,
                StockQty = 1,
                Status = ListingStatus.PendingVerification,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Listings.Add(listing4);
            await _context.SaveChangesAsync();

            _context.ListingPhotos.Add(
                new ListingPhoto { ListingId = listing4.Id, PhotoUrl = "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=800", IsCover = true, PhotoType = "FRONT", CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();
        }

        // 9. Bookings & Payments
        var booking1 = await _context.Bookings.FirstOrDefaultAsync(b => b.UserId == customer1.Id && b.ListingId == listing1.Id);
        if (booking1 == null)
        {
            booking1 = new Booking
            {
                UserId = customer1.Id,
                ListingId = listing1.Id,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(8)),
                Status = BookingStatus.Confirmed,
                SecurityDepositAmount = 10000.00m,
                TotalAmount = 12600.00m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Bookings.Add(booking1);
            await _context.SaveChangesAsync();

            _context.Payments.Add(new Payment
            {
                BookingId = booking1.Id,
                Amount = 22600.00m,
                PaymentStatus = PaymentStatus.Success,
                TransactionId = "TXN_RZP_9823412",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        var booking2 = await _context.Bookings.FirstOrDefaultAsync(b => b.UserId == customer2.Id && b.ListingId == listing2.Id);
        if (booking2 == null)
        {
            booking2 = new Booking
            {
                UserId = customer2.Id,
                ListingId = listing2.Id,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(6)),
                Status = BookingStatus.Alteration,
                SecurityDepositAmount = 15000.00m,
                TotalAmount = 22000.00m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Bookings.Add(booking2);
            await _context.SaveChangesAsync();

            _context.Payments.Add(new Payment
            {
                BookingId = booking2.Id,
                Amount = 37000.00m,
                PaymentStatus = PaymentStatus.Success,
                TransactionId = "TXN_RZP_7718291",
                CreatedAt = DateTime.UtcNow
            });

            _context.TailorTasks.Add(new TailorTask
            {
                BookingId = booking2.Id,
                TailorId = tailor.Id,
                Status = TailorTaskStatus.InProgress,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        return Ok(new
        {
            message = "Business model demo data seeded successfully!",
            supplyModels = new[] { SupplyType.AdminOwn, SupplyType.B2BShop, SupplyType.P2PIndividual },
            usersCreated = 8,
            listingsCreated = 4,
            bookingsCreated = 2
        });
    }
}
