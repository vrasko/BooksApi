using System.ComponentModel.DataAnnotations;

namespace BooksShared.Models
{
  public class Loan
  {
    public long? Id { get; set; }
    public long? Book_Id { get; set; } //FK
    public long? Cust_Id {  get; set; }//FK Customer ID
    //for this example using sqlite(which doesn't know Date types) Datetime is stored as TEXT Datatype as ISO8601 strings ("YYYY-MM-DD HH:MM:SS.SSS"). 
    public DateTime LoanDate { get; set; } = DateTime.Now; //Date of book lending
    public DateTime? RetDate { get; set; } //Date of book returning
    public DateTime DueDate { get; set; } = DateTime.Now.AddMonths(1); //Date of deadline for returning the book, for example one month later
    [DisplayFormat(DataFormatString = "{0:C}")]
    public float? LateFine { get; set; } // late payment penalty
  }
}
