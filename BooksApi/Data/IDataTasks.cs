using BooksShared.Models;

namespace BooksApi.Data
{
  public interface IDataTasks
  {
    Task<long?> GetAuthorId(Author author);
    Task<Book?> GetNumOfPrint(Book book);
    Task<dynamic> InsBook(Author author, Book book);
    Task<int?> InsNewAuthor(Author author);
    Task<int?> InsNewBook(Book book);
    Task<int?> UpdBookNumb(long idBook, int prints);
  }
}