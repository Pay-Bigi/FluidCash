namespace FluidCash.Models;

public class EmailTemplate : BaseEntity
{
    public string TemplateName { get; set; }
    public string? Template { get; set; }
}
