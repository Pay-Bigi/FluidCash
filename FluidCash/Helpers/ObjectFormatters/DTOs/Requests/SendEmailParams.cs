namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record SendEmailParams
{
    public string? senderEmail { get; init; }
    public string? callingEndpoint { get; set; }
    public string? organizationLogoUrl { get; init; }
    public string? organizationName { get; init; }
    public string? organizationWebUrl { get; init; }
    public string? linkedinHandleUrl { get; init; }
    public string? twitterHandleUrl { get; init; }
    public string? mailSubject { get; init; }
    public string? mailBody { get; init; }
    public bool isHtml { get; init; }
    public string? mailTemplateName { get; set; }
    public ICollection<EmailRecipientInfo>? mailRecipientsInfo { get; init; }
    public string[]? fileAttachmentPaths { get; init; }
}
public record EmailRecipientInfo
    (
        string? recipientName,
        string? recipientMailAddress
    );