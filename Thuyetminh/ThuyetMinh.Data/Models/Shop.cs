namespace ThuyetMinh.Data.Models;

public class Shop
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double ActivationRadiusMeters { get; set; }

    public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;
    public long? PublishedVersionId { get; set; }

    public ICollection<ShopSubmission> Submissions { get; set; } = new List<ShopSubmission>();
}