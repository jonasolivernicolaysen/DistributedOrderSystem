using Microsoft.EntityFrameworkCore;
using PaymentService.Models;
using SharedContracts;

namespace PaymentService.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options) { }

        public DbSet<PaymentModel> Payments { get; set; }
        public DbSet<PaymentItem> PaymentItems { get; set; }
        public DbSet<ProcessedOrder> ProcessedOrders { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentModel>()
                .HasMany(p => p.Items)
                .WithOne()
                .HasForeignKey(pi => pi.PaymentId);
        }
    }
}
