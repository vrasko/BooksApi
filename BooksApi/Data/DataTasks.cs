using BooksApi.DbAccess;
using BooksShared.Models;
namespace BooksApi.Data
{
  public class DataTasks : IDataTasks
  {
    private readonly IConfiguration _config;
    private readonly ISqliteDbAccess _sqlite;
    private readonly string _connName = "BooksApiSQLite";
    public DataTasks(IConfiguration config, ISqliteDbAccess sqlite)
    {
      _config = config;
      _sqlite = sqlite;
    }
    /// <summary>
    /// complex workaround about new book evidence
    /// </summary>
    /// <param name="author"></param>
    /// <param name="book"></param>
    /// <returns>string message for frontend UI</returns>
    public async Task<dynamic> InsBook(Author author, Book book) //parametre json
    {
      //this complex method is for inserting a new book. The target database resolves its procedure if the book is in database. if it is, DB procedure increments only number of prints. If there is no print of the book, the new record will be added. 
      //It is not efficient resolve the process of testing existence of book in api better practise is DB procedure.
      // test if book is in DB, return number of prints e.g. search by ean, can be other criteria
      Book? bookIs = await GetNumOfPrint(book);
      string retMsg = "";
      if (bookIs == null) //book isn't in the DB
      {
        //test existence of author in the db
        long? authIs = await GetAuthorId(author);// in the different (better) scenario can author.Id value come from UI, where could be author chosen from search/autocomplete window
        if (authIs == null) //the author is not in DB thus will be added new record
        {
          int? insertedA = await InsNewAuthor(author);

          if (insertedA == 1)//inserted 1 row
          {
            //get new ID from DB primary key
            long? authNewId = await GetAuthorId(author);
            book.Author_Id = authNewId;
          }
          retMsg = $"The author of this book has been recorded for the first time.";
        }
        else //author is in the DB
          book.Author_Id = authIs;
        // validating number of added print should be validated on frontend form
        book.Prints_num = book.Prints_num == 0 ? 1 : book.Prints_num;//set number of prints - first print min 1
        //insert new book record
        int? insertedB = await InsNewBook(book);
        if (insertedB == 1)
          retMsg += $" The book has been recorded for the first time.";
        else
          retMsg += $" Inserting the book record failed.";
      }
      else //book is in the DB, update the book with increment number of print added
      {
        // validating number of added print should be validated on frontend form
        book.Prints_num = bookIs.Prints_num + (book.Prints_num == 0 ? 1 : book.Prints_num);//set number of added prints of existing book
        int? updatedB = await UpdBookNumb((long)bookIs.Id, book.Prints_num);
        if (updatedB == 1)
          retMsg = "The book already exist. Number of print updated.";
        else
          retMsg = "Number of print update failed.";
      }
      return retMsg;
    }

    public async Task<Book?> GetNumOfPrint(Book book)
    {
      //reeturns ID, number of print of exixsting book, or null if not found
      string sql = "SELECT id, prints_num FROM Books  WHERE ean_barcode = :Ean_barcode"; // 9999000083137, Marína
      Book? bookIs;
      try
      {
        bookIs = await _sqlite.LoadDataOneTypeAsync<Book, dynamic>(sql, new { Ean_barcode = book.Ean_barcode }, _connName);
        return bookIs;
      }
      catch (Exception ex)
      {
        //some log logic
        return null;
      }

    }
    /// <summary>
    /// returns ID of exixsting author, or null if not found
    /// </summary>
    /// <param name="author"></param>
    /// <returns>Id of author</returns>
    public async Task<long?> GetAuthorId(Author author)
    {
      //returns ID of exixsting author, or null if not found
      //Note: functions UPPER(), LOWER() do not work with accents in Sqlite, only with ASCII chars therefore i haven't used them for this simple solution.
      //fields are joined because of possibility varios writing multiword names/surnames/pseudonyms to relevant fields. There could be various ways to reslove it in the project (using %LIKE% for each param in joined fields). 
      string sql = "SELECT id FROM Authors WHERE REPLACE(name,' ','') || REPLACE(surname,' ','') = :Name || :Surname"; //REPLACE() used due to multi word names 

      long? authIs;
      try
      {
        var res = await _sqlite.ExecScalarAsync<dynamic>(sql, new { Name = (author.Name).Replace(" ", ""), Surname = (author.Surname).Replace(" ", "") }, _connName);
        //if(res != null)
        authIs = (long?)res;
        return authIs;
      }
      catch (Exception ex)
      {
        //some log logic
        return null;
      }
    }
    /// <summary>
    /// Insert new record into table Authors
    /// </summary>
    /// <param name="author">model object Author</param>
    /// <returns>int number of record</returns>
    public async Task<int?> InsNewAuthor(Author author)
    {
      string sql = "INSERT INTO Authors (name,surname,description, nationality, century) VALUES(:Name,:Surname,:Description, :Nationality, :Century)";
      _sqlite.StartTransaction(_connName);
      var res = await _sqlite.SaveDataInTransactionAsync(sql, author);
      _sqlite.CommitTransaction();
      return res;
    }
    /// <summary>
    /// Insert new Books record
    /// </summary>
    /// <param name="book"></param>
    /// <returns>int number of records</returns>
    public async Task<int?> InsNewBook(Book book)
    {
      string sql = "INSERT INTO Books (title, description, author_id, publisher, edition_year, isbn, ean_barcode, prints_num, note) VALUES(:Title, :Description, :Author_Id, :Publisher, :Edition_year, :Isbn, :Ean_barcode, :Prints_num, :Note)";
      //string sql = "INSERT INTO Books (title, description, author_id, publisher, edition_year, isbn, ean_barcode, prints_num, note) VALUES(:Title, :Description, 3, :Publisher, :Edition_year, :Isbn, :Ean_barcode, :Prints_num, :Note)";
      _sqlite.StartTransaction(_connName);
      var res = await _sqlite.SaveDataInTransactionAsync(sql, book);
      _sqlite.CommitTransaction();
      return res;
    }
    public async Task<int?> UpdBook(Book book)
    {
      string sql = "UPDATE Books SET title = :Title, description = :Description, author_id = :Author_id, publisher = :Publisher, edition_year = :Edition_year, isbn = :Isbn, ean_barcode = :Ean_barcode, prints_num = :Prints_num, note = :Note WHERE id = :Id";
      _sqlite.StartTransaction(_connName);
      var res = await _sqlite.SaveDataInTransactionAsync(sql, book);
      _sqlite.CommitTransaction();
      return res;
    }
    public async Task<int?> UpdBookNumb(long idBook, int prints)
    {
      string sql = "UPDATE Books SET prints_num = :Prints_num  WHERE  id = :Id";
      try
      {
        int? res = await _sqlite.ExecAsync<dynamic>(sql, new { Prints_num = prints, Id = idBook }, _connName);
        return res;
      }
      catch (Exception ex)
      {
        //some log logic
        return null;
      }
    }
  }
}
