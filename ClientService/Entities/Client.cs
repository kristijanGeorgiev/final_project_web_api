namespace ClientService.Entities;
public class Client
{
    public int Id { get; set; }
    public int ClientTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxNumber { get; set; }
}
