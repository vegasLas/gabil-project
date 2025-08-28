using SQLite;
using System.ComponentModel.DataAnnotations;

namespace PoligonMaui.Models;

public class TargetGroup
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public double CenterLatitude { get; set; }

    public double CenterLongitude { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation property - ignored by SQLite
    [Ignore]
    public List<Target> Targets { get; set; } = new List<Target>();
}