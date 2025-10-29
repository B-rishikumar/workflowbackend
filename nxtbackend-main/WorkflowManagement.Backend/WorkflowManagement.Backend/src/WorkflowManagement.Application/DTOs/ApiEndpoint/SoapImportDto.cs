using System.ComponentModel.DataAnnotations;

namespace WorkflowManagement.Application.DTOs.ApiEndpoint;

public class SoapImportDto
{
    [Required]
    [Url]
    public string WsdlUrl { get; set; } = string.Empty;
    
    public string? ServiceName { get; set; }
    
    public string? PortName { get; set; }
    
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();
    
    public bool ImportAll { get; set; } = true;
    
    public IEnumerable<string> SelectedOperations { get; set; } = new List<string>();
}