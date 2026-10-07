namespace ThuyetMinh.Data.Models;

public class ShopSubmission
{
    public long Id { get; set; }
    public long ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    public long SubmittedById { get; set; }
    public User SubmittedBy { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double ActivationRadiusMeters { get; set; }
    public string? Description { get; set; }

    public string ImageUrlsRaw { get; set; } = "";

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;
    public string? RejectionReason { get; set; }

    public long? ReviewedById { get; set; }
    public User? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsUpdateRequest { get; set; }
}
