using System.ComponentModel.DataAnnotations;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Business.Dtos;

public class ShopSubmissionRequest
{
    [Required, MaxLength(200)] public string Name { get; set; } = null!;
    [Required, MaxLength(500)] public string Address { get; set; } = null!;

    [Range(-90, 90)] public double Latitude { get; set; }
    [Range(-180, 180)] public double Longitude { get; set; }
    [Range(1, 5000)] public double ActivationRadiusMeters { get; set; }

    [MaxLength(4000)] public string? Description { get; set; }
    public string ImageUrlsRaw { get; set; } = "";
    public bool Submit { get; set; }
}

public class ReviewRequest
{
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
}

public record ShopResponse(long Id, string Name, string Address,
    double Latitude, double Longitude, double ActivationRadiusMeters,
    PublishStatus PublishStatus, long? PublishedVersionId);

public record SubmissionResponse(long Id, long ShopId, SubmissionStatus Status,
    string? RejectionReason, DateTime CreatedAt, DateTime? ReviewedAt);

public record ShopContentResponse(long ShopId, string Name, string Address,
    double Latitude, double Longitude, double ActivationRadiusMeters,
    string? Description, string ImageUrlsRaw, long VersionId);

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token);