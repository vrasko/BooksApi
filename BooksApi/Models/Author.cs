using System.ComponentModel.DataAnnotations;
using BooksApi.Helpers;

namespace BooksApi.Models
{
  public class Author
  {
    //there is not solved localization and internationalization of messages in this simple example project
    [Key]
    public long Id { get; set; }
    public string? Name { get; set; } //all names except of surname in one string because of various formats
    [Required(AllowEmptyStrings = false, ErrorMessage = "add_snm_auth")]// // "Uveďte meno autora." need to resolve in method  localization appAppMessages.MsgDict[]

    public string Surname {  get; set; }//surneme separately because of indexing and sorting
    public string? Description { get; set; }
  }

  public class DataWrapper
  {
    public Author Author { get; set; }
    public Book Book { get; set; }
  }
}
