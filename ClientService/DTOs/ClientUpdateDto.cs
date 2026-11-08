using System.ComponentModel.DataAnnotations;
namespace ClientService.DTOs;
public class ClientUpdateDto
{
    [Range(1, int.MaxValue)] public int ClientTypeId { get; set; }
    [Required][StringLength(200)] public string Name { get; set; } = string.Empty;
    [StringLength(255)] public string? Address { get; set; }
    [StringLength(100)] public string? City { get; set; }
    [StringLength(50)] public string? Phone { get; set; }
    [StringLength(150)] public string? Email { get; set; }
    [StringLength(50)] public string? TaxNumber { get; set; }
}
