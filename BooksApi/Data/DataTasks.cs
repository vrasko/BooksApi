using BooksApi.DbAccess;
using BooksShared.Models;
using System.Net;
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
    /// complex workaround about new loan evidence
    /// </summary>
    /// <param name="author"></param>
    /// <param name="book"></param>
    /// <returns>string message for frontend UI</returns>
    public async Task<dynamic> InsBook(Author author, Book book) //parametre json
    {
      //this complex method is for inserting a new loan. The target database resolves its procedure if the loan is in database. if it is, DB procedure increments only number of prints. If there is no print of the loan, the new record will be added. 
      //It is not efficient resolve the process of testing existence of loan in api better practise is DB procedure.
      // test if loan is in DB, return number of prints e.g. search by ean, can be other criteria
      Book? bookIs = await GetNumOfPrint(book);
      string retMsg = "";
      if (bookIs == null) //loan isn't in the DB
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
          retMsg = $"The author of this loan has been recorded for the first time.";
        }
        else //author is in the DB
          book.Author_Id = authIs;
        // validating number of added print should be validated on frontend form
        book.Prints_num = book.Prints_num == 0 ? 1 : book.Prints_num;//set number of prints - first print min 1
        //insert new loan record
        int? insertedB = await InsNewBook(book);
        if (insertedB == 1)
          retMsg += $" The loan has been recorded for the first time.";
        else
          retMsg += $" Inserting the loan record failed.";
      }
      else //loan is in the DB, update the loan with increment number of print added
      {
        // validating number of added print should be validated on frontend form
        book.Prints_num = bookIs.Prints_num + (book.Prints_num == 0 ? 1 : book.Prints_num);//set number of added prints of existing loan
        int? updatedB = await UpdBookNumb((long)bookIs.Id, book.Prints_num);
        if (updatedB == 1)
          retMsg = "The loan already exist. Number of print updated.";
        else
          retMsg = "Number of print update failed.";
      }
      return retMsg;
    }

    public async Task<Book?> GetNumOfPrint(Book book)
    {
      //reeturns ID, number of print of exixsting loan, or null if not found
      string sql = "SELECT id, prints_num FROM Books  WHERE ean_barcode = :Ean_barcode"; // 9999000083137, Marína
      Book? bookIs;
      try
      {
        bookIs = await _sqlite.LoadDataOneTypeAsync<Book, dynamic>(sql, new { Ean_barcode = book.Ean_barcode }, _connName);
        return bookIs;
      }
      catch (Exception ex)
      {
        //some logic for log
        return null;
      }
    }

    public async Task<Book?> GetBookInf(long bookid)
    {
      //reeturns ID, number of print of exixsting loan, or null if not found
      string sql = "SELECT * FROM Books  WHERE id = :Bookid"; //1
      Book? bookIs;
      try
      {
        bookIs = await _sqlite.LoadDataOneTypeAsync<Book, dynamic>(sql, new { Bookid = bookid }, _connName);
        return bookIs;
      }
      catch (Exception ex)
      {
        //some logic for log
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
       _sqlite.StartTransaction(_connName);
      var res = await _sqlite.SaveDataInTransactionAsync(sql, book);
      _sqlite.CommitTransaction();
      return res;
    }
    public async Task<int?> UpdBook(Book book)
    {
      // if foreign constraint restrict in Sqlite is needed to prevent update non existing author_id , there must send command  [PRAGMA foreign_keys = ON;] after open connection. For the example purpose it is not necessary.
      //Author_Id is updating if it its value > 0, else it will not change.

      string sql = "UPDATE Books SET title = :Title, description = :Description, author_id = CASE WHEN :Author_Id > 0 THEN :Author_Id  ELSE author_id END, publisher = :Publisher, edition_year = :Edition_year, isbn = :Isbn, ean_barcode = :Ean_barcode, prints_num = :Prints_num, note = :Note WHERE id = :Id";
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
        //some log logic ...
        return null;
      }
    }
    /// <summary>
    /// delete record from table given by Id
    /// </summary>
    /// <param name="recId">the name of Id column must be "id"</param>
    /// <param name="tableName">name of the target table</param>
    /// <returns></returns>
    public async Task<int?> DeleteRec(long recId, string tableName)
    {
      try
      {
        string sql = $"delete from {tableName} where id = :RecId";
        var res = await _sqlite.ExecAsync<dynamic>(sql, new { RecId = recId }, _connName);
        return res;
      }
      catch (Exception ex)
      {
        //some log logic ...
        return null;
      }
    }
    /// <summary>
    /// Insert new Loan record
    /// </summary>
    /// <param name="loan"></param>
    /// <returns>int number of records</returns>
    public async Task<int?> NewLoanIns(Loan loan)
    {
      string sql = "INSERT INTO Loans (book_id, cust_id, loandate, duedate) VALUES(:Book_Id, :Cust_Id, :LoanDate, :DueDate)";
      _sqlite.StartTransaction(_connName);
      var res = await _sqlite.SaveDataInTransactionAsync(sql, loan);
      _sqlite.CommitTransaction();
      return res;
    }
  }
}
