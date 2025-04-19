using FluidCash.DataAccess.DbContext;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IExternalServicesRepo;
using FluidCash.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace FluidCash.ExternalServicesRepo;

public class EmailSender : IEmailSender
{
    private readonly IEmailProvider _emailProvider;
    private readonly DbSet<EmailTemplate> _emailTemplates;
    private readonly DbSet<EmailLog> _emailLogs;
    private DataContext _dataContext;

    public EmailSender(IEmailProvider emailProvider, DataContext dataContext)
    {
        _emailProvider = emailProvider;
        _emailTemplates = dataContext.EmailTemplates;
        _emailLogs = dataContext.EmailLogs;
        _dataContext = dataContext;
    }
    public async Task<StandardResponse<string>>
        ProcessAndSendEmailAsync
        (SendEmailParams requestDto)
    {
        const string defaultTemplateName = "Default Mail Template";

        requestDto.mailTemplateName = string.IsNullOrWhiteSpace(requestDto.mailTemplateName) ?
            defaultTemplateName : requestDto.mailTemplateName;

        var emailTemplate = await _emailTemplates.Where(mailTemp => mailTemp.TemplateName == requestDto.mailTemplateName)
            .Select(mailTemp => mailTemp.Template)
            .SingleOrDefaultAsync();

        if (emailTemplate is null)
        {
            return StandardResponse<string>.Failed(data: "Email template not found", statusCode: 400);
        }

        await CustomizeAndSendMailAsync(requestDto, emailTemplate);

        return StandardResponse<string>.Success(data: "Mail Request sent successfully", 200);
    }

    private async Task
        CustomizeAndSendMailAsync
        (SendEmailParams sendEmailRequestDto, string emailTemplate)
    {
        var emailLogs = new List<EmailLog>();
        foreach (var recipent in sendEmailRequestDto.mailRecipientsInfo!)
        {
            var recipientName = recipent.recipientName;
            var messageBody = sendEmailRequestDto.mailBody;
            var orgLinkedinUrl = sendEmailRequestDto.linkedinHandleUrl;
            var orgLogoUrl = sendEmailRequestDto.organizationLogoUrl;
            var orgTwitterUrl = sendEmailRequestDto.twitterHandleUrl;
            var orgWebUrl = sendEmailRequestDto.organizationWebUrl;
            var recipentEmailAddress = recipent.recipientMailAddress;
            var orgName = sendEmailRequestDto.organizationName;
            var mailSubject = orgName + ": " + sendEmailRequestDto.mailSubject;
            var isHtml = sendEmailRequestDto.isHtml;
            var senderMail = sendEmailRequestDto.senderEmail;
            var year = DateTime.UtcNow.AddHours(1).Year.ToString();
            var fileAttachmentPaths = sendEmailRequestDto.fileAttachmentPaths;

            emailTemplate = emailTemplate.Replace("{{org-url}}", orgWebUrl);
            emailTemplate = emailTemplate.Replace("{{twitterurl}}", orgTwitterUrl);
            emailTemplate = emailTemplate.Replace("{{linkedinurl}}", orgLinkedinUrl);
            emailTemplate = emailTemplate.Replace("{{body}}", messageBody);
            emailTemplate = emailTemplate.Replace("{{recipient}}", recipientName);
            emailTemplate = emailTemplate.Replace("{{logo}}", orgLogoUrl);
            emailTemplate = emailTemplate.Replace("{{receiverEmail}}", recipentEmailAddress);
            emailTemplate = emailTemplate.Replace("{{sendermail}}", senderMail);
            emailTemplate = emailTemplate.Replace("{{year}}", year);
            emailTemplate = emailTemplate.Replace("{{org-name}}", orgName);

            EmailParams emailRequest = new EmailParams()
            {
                SenderEmail = senderMail!,
                EmailSubject = mailSubject,
                EmailBody = emailTemplate,
                RecipientEmail = recipentEmailAddress!,
                SenderName = orgName!,
                IsHtml = isHtml,
                FileAttachmentPaths = fileAttachmentPaths
            };

            var result = _emailProvider.SendEmail(emailRequest);

            var emailLog = new EmailLog()
            {
                CreatedBy = "Application System",
                CallingEndpoint = sendEmailRequestDto.callingEndpoint
            };

            if (!result.Succeeded)
            {
                emailLog.IsSuccessful = false;
                emailLog.Detail = result.Errors.FirstOrDefault()!.Description;
                emailLog.SenderName = orgName;
                emailLog.RecipientEmail = recipentEmailAddress;
            }
            else
            {
                string successDetail = "Mail Sent Successfully";
                emailLog.IsSuccessful = false;
                emailLog.Detail = successDetail;
                emailLog.SenderName = orgName;
                emailLog.RecipientEmail = recipentEmailAddress;
            }
            emailLogs.Add(emailLog);
        }
        await _emailLogs.AddRangeAsync(emailLogs);
        await _dataContext.SaveChangesAsync();
    }
}
