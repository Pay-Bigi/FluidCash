namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record CreateUserAccountParams
{
    public string? dpUrl {  get; set; } 
    public string? dpCloudinaryId { get; set; }
    public string? displayName {  get; set; }
    public string? appUserId { get; set; }
}