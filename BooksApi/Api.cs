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
    }

    private static async Task<IResult> InsertBook(IDataTasks dataTasks, HttpContext context)//, ILoggerFactory loggerFactory) // post - nepouzivam asynchronny rezim
    {
      //ILogger log = loggerFactory.CreateLogger("InsNewBook");
      try
      {
        var collect = await context.Request.ReadFromJsonAsync<DataWrapper>();
        if (collect != null)
        {
          //if author is in DB, the ID is not null!!
          var author = collect.Author;
          var book = collect.Book;

          var res = await dataTasks.InsNewBook(author, book);
          return Results.Ok(res);
        }
        else
          return Results.NoContent();
      }
      catch (Exception ex)
      {
        //log.LogError(errmess, ex.Message);
        return Results.Text(_errorIndicator + ex.Message);
      }
    }
  }

}
