namespace FluidCash.Models;

public class BaseEntity
{
    public string? Id { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
