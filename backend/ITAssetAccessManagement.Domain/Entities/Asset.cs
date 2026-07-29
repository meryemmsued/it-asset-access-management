using ITAssetAccessManagement.Domain.Common;
using ITAssetAccessManagement.Domain.Enums;

namespace ITAssetAccessManagement.Domain.Entities;

public sealed class Asset : AuditableEntity
{
    public Guid AssetCategoryId { get; set; }

    public string AssetCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public AssetStatus Status { get; set; } = AssetStatus.Available;

    public DateOnly? PurchaseDate { get; set; }

    public decimal? PurchasePrice { get; set; }

    public DateOnly? WarrantyExpirationDate { get; set; }

    public Guid CreatedByUserId { get; set; }

    public AssetCategory AssetCategory { get; set; } = null!;

    public User CreatedByUser { get; set; } = null!;

    public PhysicalAssetDetail? PhysicalDetail { get; set; }

    public DigitalAssetDetail? DigitalDetail { get; set; }

    public ICollection<AssetAssignment> Assignments { get; set; }
        = new List<AssetAssignment>();

    public ICollection<AssetStatusHistory> StatusHistories { get; set; }
        = new List<AssetStatusHistory>();

    public ICollection<AccessRequest> AccessRequests { get; set; }
        = new List<AccessRequest>(); 

}