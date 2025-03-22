namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public class EmailParams
{
    public string SenderEmail { get; set; }
    public string EmailSubject { get; set; }
    public string EmailBody { get; set; }
    public string RecipientEmail { get; set; }
    public string SenderName { get; set; }
    public bool IsHtml { get; set; }
    public string[]? FileAttachmentPaths { get; set; }
}
