using BooksApi.DbAccess;
using BooksShared.Models;
using BooksApi.Helpers;

namespace BooksApi.Data
{
  public class DataTasks : IDataTasks
  {
    private readonly IConfiguration _config;
    private readonly ISqliteDbAccess _sqlite;
    private readonly string _connName = "BooksApiSQLite";
    private readonly EmailSender _emailSender;
    public DataTasks(IConfiguration config, ISqliteDbAccess sqlite, EmailSender emailSender)
    {
      _config = config;
      _sqlite = sqlite;
      _emailSender = emailSender;
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
        //some logic for log
        return null;
      }
    }

    public async Task<Book?> GetBookInf(long bookid)
    {
      //reeturns ID, number of print of exixsting book, or null if not found
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
    /// <summary>
    /// retrieves data for writing the confirmation of returning the book. 
    /// </summary>
    /// <param name="bookid">ID of the book which is the confirmation for</param>
    /// <returns>model object ComplexModel or null If date of return is empty</returns>
    public async Task<ComplexView?> GetRetConfirm(long bookid)
    {
      //returns complex model for the book return confirmation given by book Id. Date of return must not be null
      string sql = "SELECT c.title C_Title, c.name C_Name,c.surname C_Surname, a.name A_Name, a.surname A_Surname,b.title B_Title,l.loandate L_LoanDate,l.retdate L_RetDate " +
        "FROM Customers c, Authors a ,Books b ,Loans l " +
        "WHERE l.retdate NOTNULL AND TRIM(l.retdate) !='' AND l.cust_id = c.id AND l.book_id = b.id AND b.author_id=a.id and b.id= :Bookid";
      ComplexView? cw;
      try
      {
        cw = await _sqlite.LoadDataOneTypeAsync<ComplexView, dynamic>(sql, new { Bookid = bookid }, _connName);


        return cw;
      }
      catch (Exception ex)
      {
        //some logic for log
        return null;
      }
    }

    /// <summary>
    /// Retrieves not returned books and necessary data for reminders which due date is tomorrow.
    /// </summary>
    /// <returns></returns>
    public async Task<string?> SendRem()
    {
      //DateOnly tomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
      //day tomorrow is resolved in build-db functions

      //returns complex model for the book return reminder Date of return must  be null
      string sql = "SELECT c.title C_Title, c.name C_Name,c.surname C_Surname, c.ct_mail C_Ct_Mail, a.name A_Name, a.surname A_Surname,b.title B_Title,l.loandate L_LoanDate " +
        "FROM Customers c, Authors a ,Books b ,Loans l " +
        "WHERE (l.retdate ISNULL OR TRIM(l.retdate) ='') AND l.cust_id = c.id AND l.book_id = b.id AND b.author_id=a.id " +
        "AND DATE(l.duedate) = DATE(DATETIME('now','+2 hours'), '+1 day')";
      //Warning. Sqlite works default with UTC Time zone, so for Slovak Summer time must be 2 hors added
      List<ComplexView?>? cws;
      try
      {
        cws = await _sqlite.LoadListTypeAsync<ComplexView?>(sql, null, _connName);

        string emailRemindBodyTemplate = "Váž. p. {0} {1},\r\n Dovoľujeme si Vás upozorniť, že zajtra uplynie čas, na ktorý ste si požičali knihu {2} {3}: {4} dňa {5}. Prosíme Vás o jej vrátenie.\r\nVaša knižnica";

        EmailModel emod = new();
        emod.Subject = "Pripomienka";

        foreach (var cw in cws)
        {
          emod.To = cw.C_Ct_mail;
          emod.Body = string.Format(emailRemindBodyTemplate, cw.C_Name, cw.C_Surname, cw.A_Name, cw.A_Surname, cw.B_Title, cw.L_LoanDate);
          await _emailSender.SendEmail(emod);
        }
        return cws.Count.ToString();
      }
      catch (Exception ex)
      {
        //some logic for log
        return null;
      }
    }
  }
}
