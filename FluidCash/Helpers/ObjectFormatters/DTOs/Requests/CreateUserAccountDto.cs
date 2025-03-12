namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record CreateUserAccountDto
{
    public string? dpUrl {  get; set; } 
    public string? dpCloudinaryId { get; set; }
    public string? displayName {  get; set; }
    public string? appUserId { get; set; }
}