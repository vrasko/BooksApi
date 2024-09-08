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
      //Add new book - Vytvorenie novej knihy
      app.MapPost("/insertbook", InsertBook);
      // Získanie detailov existujúcej knihy podľa ID - Get book info
      app.MapGet("/getbookinfo/{bookid}", GetBookInfo);
      //Aktualizáciu existujúcej knihy - Update an existing book
      app.MapPost("/updbook", UpdateBook);
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
  }

}
