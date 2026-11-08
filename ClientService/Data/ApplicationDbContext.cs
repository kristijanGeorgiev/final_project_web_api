using Microsoft.EntityFrameworkCore;
using ClientService.Entities;
namespace ClientService.Data;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Client>();
        e.ToTable("Clients");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedOnAdd();
        e.Property(x => x.Name).HasMaxLength(200).IsRequired();
        e.Property(x => x.Address).HasMaxLength(255);
        e.Property(x => x.City).HasMaxLength(100);
        e.Property(x => x.Phone).HasMaxLength(50);
        e.Property(x => x.Email).HasMaxLength(150);
        e.Property(x => x.TaxNumber).HasMaxLength(50);
    }
}
