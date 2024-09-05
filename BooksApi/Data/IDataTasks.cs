using BooksApi.Models;

namespace BooksApi.Data
{
  public interface IDataTasks
  {
    Task<dynamic> InsNewBook(Author author, Book book);
  }
}