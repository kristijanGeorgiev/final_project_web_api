using System.ComponentModel.DataAnnotations;
namespace ClientTypeService.DTOs;
public class ClientTypeCreateDto
{
    [Required][StringLength(100)] public string Name { get; set; } = string.Empty;
    [StringLength(255)] public string? Description { get; set; }
}
