// ApiEndpoint/SwaggerImportDto.cs
using System.ComponentModel.DataAnnotations;

namespace WorkflowManagement.Application.DTOs.ApiEndpoint;

public class SwaggerImportDto
{
    [Required]
    [Url]
    public string SwaggerUrl { get; set; } = string.Empty;
    
    public string? BaseUrl { get; set; }
    
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();
    
    public bool ImportAll { get; set; } = true;
    
    public IEnumerable<string> SelectedPaths { get; set; } = new List<string>();
}
