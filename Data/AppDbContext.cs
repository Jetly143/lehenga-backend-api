using Microsoft.EntityFrameworkCore;
using Rent_Lehenga_Project.Models;

namespace Rent_Lehenga_Project.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ListingPhoto> ListingPhotos => Set<ListingPhoto>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PayoutBatch> PayoutBatches => Set<PayoutBatch>();
    public DbSet<PayoutItem> PayoutItems => Set<PayoutItem>();
    public DbSet<LogisticsTask> LogisticsTasks => Set<LogisticsTask>();
    public DbSet<LogisticsTaskPhoto> LogisticsTaskPhotos => Set<LogisticsTaskPhoto>();
    public DbSet<TailorTask> TailorTasks => Set<TailorTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USERS
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).HasDefaultValue(UserRole.Customer);
            entity.Property(u => u.KycStatus).HasDefaultValue(KycStatus.NotSubmitted);
            entity.Property(u => u.IsVerified).HasDefaultValue(false);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(u => u.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // LISTINGS
        modelBuilder.Entity<Listing>(entity =>
        {
            entity.Property(l => l.StockQty).HasDefaultValue(1);
            entity.Property(l => l.SupplyType).HasDefaultValue(SupplyType.AdminOwn);
            entity.Property(l => l.Category).HasDefaultValue("BRIDAL");
            entity.Property(l => l.SecurityDeposit).HasDefaultValue(0.00m);
            entity.Property(l => l.OriginalPrice).HasDefaultValue(0.00m);
            entity.Property(l => l.Status).HasDefaultValue(ListingStatus.PendingVerification);
            entity.Property(l => l.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(l => l.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(l => l.Owner)
                .WithMany(u => u.Listings)
                .HasForeignKey(l => l.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LISTING PHOTOS
        modelBuilder.Entity<ListingPhoto>(entity =>
        {
            entity.Property(p => p.IsCover).HasDefaultValue(false);
            entity.Property(p => p.PhotoType).HasDefaultValue("FRONT");
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(p => p.Listing)
                .WithMany(l => l.Photos)
                .HasForeignKey(p => p.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // BOOKINGS
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(b => b.Status).HasDefaultValue(BookingStatus.Pending);
            entity.Property(b => b.SecurityDepositAmount).HasDefaultValue(0.00m);
            entity.Property(b => b.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(b => b.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Listing)
                .WithMany(l => l.Bookings)
                .HasForeignKey(b => b.ListingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PAYMENTS
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(p => p.TransactionId).IsUnique();
            entity.Property(p => p.PaymentStatus).HasDefaultValue(PaymentStatus.Pending);
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // NOTIFICATIONS
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(n => n.IsRead).HasDefaultValue(false);
            entity.Property(n => n.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PAYOUT_BATCHES
        modelBuilder.Entity<PayoutBatch>(entity =>
        {
            entity.Property(pb => pb.TotalAmount).HasDefaultValue(0.00m);
            entity.Property(pb => pb.Status).HasDefaultValue(PayoutBatchStatus.Pending);
            entity.Property(pb => pb.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(pb => pb.Owner)
                .WithMany(u => u.PayoutBatches)
                .HasForeignKey(pb => pb.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PAYOUT_ITEMS
        modelBuilder.Entity<PayoutItem>(entity =>
        {
            entity.HasIndex(pi => pi.BookingId).IsUnique();
            entity.Property(pi => pi.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(pi => pi.PayoutBatch)
                .WithMany(pb => pb.Items)
                .HasForeignKey(pi => pi.PayoutBatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pi => pi.Booking)
                .WithOne(b => b.PayoutItem)
                .HasForeignKey<PayoutItem>(pi => pi.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LOGISTICS_TASKS
        modelBuilder.Entity<LogisticsTask>(entity =>
        {
            entity.Property(lt => lt.Status).HasDefaultValue(LogisticsTaskStatus.Pending);
            entity.Property(lt => lt.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(lt => lt.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(lt => lt.Booking)
                .WithMany(b => b.LogisticsTasks)
                .HasForeignKey(lt => lt.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(lt => lt.AssignedUser)
                .WithMany(u => u.AssignedLogisticsTasks)
                .HasForeignKey(lt => lt.AssignedTo)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LOGISTICS_TASK_PHOTOS
        modelBuilder.Entity<LogisticsTaskPhoto>(entity =>
        {
            entity.Property(p => p.Status).HasDefaultValue(LogisticsTaskStatus.Pending);
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(p => p.Task)
                .WithMany(t => t.Photos)
                .HasForeignKey(p => p.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TAILOR_TASKS
        modelBuilder.Entity<TailorTask>(entity =>
        {
            entity.Property(tt => tt.Status).HasDefaultValue(TailorTaskStatus.Pending);
            entity.Property(tt => tt.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(tt => tt.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(tt => tt.Booking)
                .WithMany(b => b.TailorTasks)
                .HasForeignKey(tt => tt.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tt => tt.Tailor)
                .WithMany(u => u.AssignedTailorTasks)
                .HasForeignKey(tt => tt.TailorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
