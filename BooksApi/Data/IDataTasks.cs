using BooksShared.Models;

namespace BooksApi.Data
{
  public interface IDataTasks
  {
    Task<int?> DeleteRec(long recId, string tableName);
    Task<long?> GetAuthorId(Author author);
    Task<Book?> GetBookInf(long bookid);
    Task<Book?> GetNumOfPrint(Book book);
    Task<ComplexView?> GetRetConfirm(long bookid);
    Task<dynamic> InsBook(Author author, Book book);
    Task<int?> InsNewAuthor(Author author);
    Task<int?> InsNewBook(Book book);
    Task<int?> NewLoanIns(Loan loan);
    Task<string?> SendRem();
    Task<int?> UpdBook(Book book);
    Task<int?> UpdBookNumb(long idBook, int prints);
  }
}