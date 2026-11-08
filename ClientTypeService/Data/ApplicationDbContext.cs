using Microsoft.EntityFrameworkCore;
using ClientTypeService.Entities;
namespace ClientTypeService.Data;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ClientType> ClientTypes => Set<ClientType>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<ClientType>();
        e.ToTable("ClientTypes");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedOnAdd();
        e.Property(x => x.Name).HasMaxLength(100).IsRequired();
        e.Property(x => x.Description).HasMaxLength(255);
    }
}
