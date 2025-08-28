using SQLite;
using System.ComponentModel.DataAnnotations;

namespace PoligonMaui.Models;

public class Target
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    [Required]
    public string Color { get; set; } = "#FF0000"; // Default red

    public bool IsReached { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TargetGroupId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Navigation property
    [Ignore]
    public TargetGroup? TargetGroup { get; set; }
}