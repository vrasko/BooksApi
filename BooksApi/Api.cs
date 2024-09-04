using BooksApi.Models;
namespace BooksApi
{
  public static class Api
  {
    public static void ConfigureApi(this WebApplication app)
    {
      //Add new book - Vytvorenie novej knihy
      app.MapPost("/addnewbook", InsNewBook);
    }

    private static async Task<IResult> InsNewBook(IDataTasks dataTasks, HttpContext context, ILoggerFactory loggerFactory) // post - nepouzivam asynchronny rezim
    {
      ILogger log = loggerFactory.CreateLogger("InsertDodavatel");
      try
      {
        IFormCollection jsons = await context.Request.ReadFormAsync(); //pomenovane jsons key, value
        //vsetky keys len jedna hodnota, nie pole
        string osoba = jsons["osoba"]; //[0];
        string dod = jsons["dodavatel"];// [0]; //json
        //long did = long.Parse(jsons["did"]);// ;
        var res = await dataTasks.InsertDodavatel(osoba, dod);
        if (dod == null)
          return Results.NotFound();
        return Results.Ok(res);
      }
      catch (Exception ex)
      {
        log.LogError(errmess, ex.Message);
        return Results.Text(_errorIndicator + ex.Message);
      }
    }
  }

}
