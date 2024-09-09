using BooksApi.Data;
using BooksApi.Helpers;
using BooksShared.Models;
namespace BooksApi
{
  public static class Api
  {
    private const string errmess = "Nastala chyba, výnimka: {@par}";
    private const char _errorIndicator = Consts.errorIndicator;
    public static void ConfigureApi(this WebApplication app)
    {
      // Vytvorenie novej knihy - Add new book 
      app.MapPost("/insertbook", InsertBook);
      // Získanie detailov existujúcej knihy podľa ID - Get book info
      app.MapGet("/getbookinfo/{bookid}", GetBookInfo);
      // Aktualizáciu existujúcej knihy - Update an existing book
      app.MapPost("/updbook", UpdateBook);
      // Odstránenie knihy - Delete book
      app.MapGet("deletebook/{bookid}", DeleteBook);
      // Vytvorenie novej zápožičky - New loan
      app.MapPost("/newloan", NewLoan);
      // Potvrdenie o vrátení vypožičanej knihy - Confirmation of the return of the borrowed book
      app.MapGet("/getretconf/{bookid}", GetRetConf);
      // Pripomienka na vratenie knihy den pred dátumom vrátenia - Reminder of the return of the borrowed book
      app.MapGet("/sendreminder", SendRemind);

    }
    /// <summary>
    /// Complex routine for adding new book(s) to DB
    /// </summary>
    /// <param name="dataTasks"></param>
    /// <param name="context"></param>
    /// <returns>string message for UI</returns>
    private static async Task<IResult> InsertBook(IDataTasks dataTasks, HttpContext context, ILoggerFactory loggerFactory) 
    {
      ILogger log = loggerFactory.CreateLogger("InsNewBook");
      try
      {
        var collect = await context.Request.ReadFromJsonAsync<DataWrapper>();
        if (collect != null)
        {
          //if author is in DB, the ID is not null!!
          var author = collect.Author; 
          var book = collect.Book;

          var res = await dataTasks.InsBook(author, book);
          return Results.Ok(res);

        }
        else
        log.LogWarning("Nenačítali sa údaje z DB.");
        return Results.NoContent();
      }
      catch (Exception ex)
      {
        log.LogError(errmess, ex.Message);
        return Results.Problem(_errorIndicator + ex.Message);
      }
    }
    /// <summary>
    /// Reads data of one book given by the book ID
    /// </summary>
    /// <param name="dataTasks"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="bookid"></param>
    /// <returns>Model object Book</returns>
    private static async Task<IResult> GetBookInfo(IDataTasks dataTasks, ILoggerFactory loggerFactory, long bookid)
    {
      ILogger log = loggerFactory.CreateLogger("GetBookInfo");
      try
      {
        var results = await dataTasks.GetBookInf(bookid);
        if (results == null)
        {
          log.LogWarning("Nenačítali sa údaje z DB.");
          return Results.NotFound();
        }

        return Results.Ok(results);
      }
      catch (Exception ex)
      {
        log.LogError(errmess, ex.Message);
        return Results.Problem(_errorIndicator + ex.Message);
      }
    }
    /// <summary>
    /// Update record of existing book - suitable for editing
    /// </summary>
    /// <param name="dataTasks"></param>
    /// <param name="context">model object Book, ID required</param>
    /// <param name="loggerFactory"></param>
    /// <returns>number of updated records</returns>
    private static async Task<IResult> UpdateBook(IDataTasks dataTasks, HttpContext context, ILoggerFactory loggerFactory)
    {
      string mes;
      //ID of book is required value in object
      ILogger log = loggerFactory.CreateLogger("UpdateBook");
      try
      {
        Book? book = await context.Request.ReadFromJsonAsync<Book>();
        if (book != null)
        {
          if (book.Id != null)
          {
            var res = await dataTasks.UpdBook(book);
            return Results.Ok(res); //val 1
          }
          else // book Id is missing
          {
            mes = _errorIndicator +" Chýba ID knihy, údaje sa nedajú aktualizovať.";
            log.LogError(mes);
            return Results.Problem(mes);
          }
        }
        else
          log.LogWarning("Nenačítali sa údaje z DB.");
        return Results.NoContent();
      }
      catch (Exception ex)
      {
        log.LogError(errmess, ex.Message);
        return Results.Problem(_errorIndicator + ex.Message);
      }
    }
    /// <summary>
    /// Deletes record from table Books
    /// </summary>
    /// <param name="dataTasks"></param>
    /// <param name="recId">book ID</param>
    /// <returns></returns>
    private static async Task<IResult> DeleteBook(IDataTasks dataTasks, ILoggerFactory loggerFactory, long bookid)
    {
      ILogger log = loggerFactory.CreateLogger("DeleteBook");
      string? tabName = "Books";
      try
      {
        var dod = await dataTasks.DeleteRec(bookid, tabName);//zmazanie zanamu so zadanym ID v zadanej tabulke
        if (dod == null)
        {
          log.LogWarning("Nenačítali sa údaje z DB.");
          return Results.NotFound();
        }
        return Results.Ok(dod);
      }
      catch (Exception ex)
      {
        log.LogError(errmess, ex.Message);
        return Results.Problem(ex.Message);
      }
    }
    /// <summary>
    /// Update record of existing book - suitable for editing
    /// </summary>
    /// <param name="dataTasks"></param>
    /// <param name="context">model object Book, ID required</param>
    /// <param name="loggerFactory"></param>
    /// <returns>number of updated records</returns>
    private static async Task<IResult> NewLoan(IDataTasks dataTasks, HttpContext context, ILoggerFactory loggerFactory)
    {
      string mes;
      //ID of book and customer are required values in object
      ILogger log = loggerFactory.CreateLogger("NewLoan");
      try
      {
        Loan? loan = await context.Request.ReadFromJsonAsync<Loan>();
        if (loan != null)
        {
          if (loan.Id == null)
          {
            var res = await dataTasks.NewLoanIns(loan);
            return Results.Ok(res); //val 1
          }
          else // book Id is missing
          {
            mes = _errorIndicator + " Chyba v modeli: ID novej zápožičky sa neuvádza, generuje ho DB.";
            log.LogError(mes);
            return Results.Problem(mes);
          }
        }
        else
          log.LogWarning("Nenačítali sa údaje z DB.");
        return Results.NoContent();
      }
      catch (Exception ex)
      {
        log.LogError(errmess, ex.Message);
        return Results.Problem(_errorIndicator + ex.Message);
      }
    }

    /// <summary>
    /// Reads complex data for the confirmation of the book return given by the book ID
    /// </summary>
    /// <param name="dataTasks"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="bookid"></param>
    /// <returns>Model object Book</returns>
    private static async Task<IResult> GetRetConf(IDataTasks dataTasks, ILoggerFactory loggerFactory, long bookid)
    {
      ILogger log = loggerFactory.CreateLogger("GetRetConf");
      try
      {
        var results = await dataTasks.GetRetConfirm(bookid);
        if (results == null)
        {
          log.LogWarning("Nenačítali sa údaje z DB. Skontrolujte dátum vrátenia.");
          return Results.NotFound();
        }

        return Results.Ok(results);
      }
      catch (Exception ex)
      {
        log.LogError(errmess, ex.Message);
        return Results.Problem(_errorIndicator + ex.Message);
      }
    }


    /// <summary>
    ///  Retrieve book wich needed to be returned tomorrow, Then sends emails to the customers.
    /// </summary>
    /// <param name="dataTasks"></param>
    /// <param name="loggerFactory"></param>
    /// <returns></returns>
    private static async Task<IResult> SendRemind(IDataTasks dataTasks, ILoggerFactory loggerFactory)
    {
      ILogger log = loggerFactory.CreateLogger("GetRetConf");
      try
      {
        var results = await dataTasks.SendRem();
        if (results == null)
        {
          log.LogWarning("Nenačítali sa údaje z DB. Skontrolujte dátum vrátenia.");
          return Results.NotFound();
        }

        return Results.Ok(results);
      }
      catch (Exception ex)
      {
        log.LogError(errmess, ex.Message);
        return Results.Problem(_errorIndicator + ex.Message);
      }
    }

  }

}
