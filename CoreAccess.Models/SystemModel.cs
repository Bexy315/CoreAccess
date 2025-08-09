namespace CoreAccess.Models;

public class HealthCheckResponse
{
    public string Status { get; set; } = "Healthy";
    public long Uptime { get; set; } = 0;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = "1.0.0"; 
    public string Environment { get; set; } = "Production"; 
    public Dictionary<string, object> Checks { get; set; } = new ();
}