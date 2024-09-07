using BooksShared.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using System.Net.Http.Json;

namespace BooksApi.Tests
{
  //How to test API points
  //1. Start debugging Unit Test
  //2. While on the first line of your test code or before calling your local web api project
  //3. Right click on your web api project and Debug > Start new instance


  [TestFixture]
  public class ApiTests
  {
    private HttpClient _client = new();

    #region Setup data
    //private readonly HttpClient _client = new();
    private string _messPosted = "";
    private const string apiUrlBase = "https://localhost:7262";

    private Author author1 = new()
    {
      Name = "Stephen",
      Surname = "King",
      Description = "master of horror",
      Nationality = "American",
      Century=20

    };

    private Book book1 = new()
    {
      Title = "Misery",
      Description = "Horror",
      //Author_Id = null,
      Publisher = "Viking Press",
      Edition_year = 1987,
      Isbn = "0670813648",
      Ean_barcode = "9780450417399", //"9999000083137",  //
      Prints_num = 11,
      Note = "English original publishing"
    };

    #endregion

    /// <summary>
    /// calling complex function manipulating new book adding
    /// </summary>
    /// <returns>message for front end UI</returns>
    [Test]
    public async Task CallInsertBookTest() //vytvori novy zaznam
    {
      string apiMap = apiUrlBase + "/insertbook"; //api address of function
      string result = "";
      HttpResponseMessage httpResponseMessage;

      try
      {
        // http context for sending from UI form - two model objects
        DataWrapper multiContent = new()
        {
          Author = author1,
          Book = book1
        };
        httpResponseMessage = await _client.PostAsJsonAsync(apiMap, multiContent);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
          result = await httpResponseMessage.Content.ReadAsStringAsync();
          NUnit.Framework.Assert.That(result, Is.Not.Null);
          Console.WriteLine(result);
        }
        else
        {
          string mes = $"Failed to insert book (Status Code: {httpResponseMessage.StatusCode})";
          NUnit.Framework.Assert.Fail();
          Console.WriteLine(mes);
        }
      }
      catch (Exception ex)
      {
        //result += string.Format("Nastala chyba: {0}\n", ex.Message);
        string mes = $"An error occurred: {ex.Message}";
        Console.WriteLine(mes);
        NUnit.Framework.Assert.Fail(mes);
      }
      //return result;
    }

  }
}
