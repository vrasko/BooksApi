using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace BooksApi.Helpers
{
  public class EmailSender
  {

    private readonly ConfigEmailModel _emailConfig;
    public EmailSender(IOptions<ConfigEmailModel>emailConfig)
    {
      _emailConfig = emailConfig.Value;
    }

    /// <summary>
    /// Sending email async 
    /// </summary>
    /// <param name="emailModel">common model for sending email</param>
    /// <param name="configEmail">Config email server from appsettings</param>
    public async Task SendEmail(EmailModel emailModel)//, ConfigEmailModel configEmail)
    {
      MailMessage message = new(new(emailModel.From, emailModel.FromName), new MailAddress(emailModel.To))
      {
        Subject = emailModel.Subject,
        Body = emailModel.Body,
        IsBodyHtml = emailModel.IsBodyHtml,
      };
      if (emailModel.Cc != null)
        message.CC.Add(emailModel.Cc);
      if (emailModel.Bcc != null)
        message.Bcc.Add(emailModel.Bcc);

      if (emailModel.IsAttachments && emailModel.Attachments is not null)
      {
        foreach ((Stream stream, string name) in emailModel.Attachments)
        {
          message.Attachments.Add(new Attachment(stream, name));
        }
      }
      SmtpClient client = new()
      {
        // Credentials are necessary if the server requires the client
        // to authenticate before it will send email on the client's behalf.
        EnableSsl = _emailConfig.EnableSsl, // configEmail.EnableSsl,
        Host = _emailConfig.Host, //configEmail.Host,
        Port = _emailConfig.Port,   //configEmail.Port,
        Credentials = new NetworkCredential(_emailConfig.UserName,_emailConfig.Password)//configEmail.UserName, configEmail.Password),
      };
      try
      {
        await client.SendMailAsync(message);
      }
      catch
      {
        throw;
      }
    }

  }
}
