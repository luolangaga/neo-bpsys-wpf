using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PluginMarket.Api.Models;

public enum ReviewStatus
{
    Pending,
    Approved,
    Rejected
}

public class PluginPackage
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string Version { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string? Description { get; set; }
    public bool RequiresRestart { get; set; }
    [MaxLength(200)]
    public string FileName { get; set; } = string.Empty;
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    [MaxLength(128)]
    public string? Sha256 { get; set; }
    public DateTime UploadedAt { get; set; }
    [MaxLength(100)]
    public string? UploadedBy { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
    [MaxLength(500)]
    public string? RejectReason { get; set; }
}
