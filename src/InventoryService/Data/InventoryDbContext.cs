using Microsoft.EntityFrameworkCore;
using InventoryService.Models;
using InventoryService.Models.DTOs;

namespace InventoryService.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
            : base(options) { }

        public DbSet<InventoryModel> Inventory { get; set; }
        public DbSet<MessageDto> ProcessedMessages { get; set; }
    }
}
