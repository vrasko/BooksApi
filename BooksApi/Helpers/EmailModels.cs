namespace BooksApi.Helpers
{
  public class EmailModel
  {
    //default Mailtrap possible values
    public  string From { get; set; } = "noreply@bookapi.sk";
    public  string FromName { get; set; } = "BookApi App";
    public  string? To { get; set; }
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string? Subject { get; set; } = "Pripomienka";
    public bool IsBodyHtml { get; set; } = false;
    public string? Body { get; set; }
    public bool IsAttachments { get; set; } = false;
    public List<(Stream stream,string name)>? Attachments { get; set; }
  }

  public class ConfigEmailModel
  {
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required bool EnableSsl { get; set; }
  }
}
