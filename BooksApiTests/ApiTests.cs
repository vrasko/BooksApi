using BooksShared.Models;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace BooksApiTests
{
  //How to test API points
  //1. Start debugging Unit Test
  //2. While on the first line of your test code or before calling your local web api project
  //3. Right click on your web api project and Debug > Start new instance


  [TestFixture]
  public class ApiTests
  {
    private readonly HttpClient _client;

    #region Setup data
    //private const string apiUrlBase = "https://localhost:7262";
    public ApiTests()
    {
      _client = new()
      {
        BaseAddress = new Uri("https://localhost:7262")
      };
    }

    private readonly Author author1 = new() // for inserting
    {
      Name = "Stephen",
      Surname = "King",
      Description = "master of horror",
      Nationality = "American",
      Century = 20

    };

    private readonly Book book1 = new()  // for inserting
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

    private readonly Book bookUpd = new() //for updating
    {
      Id = 2,
      Note = "English language",
      Description = "Horror & Mystery genre"
    };

    private readonly long bookInfoParam = 1;

    #endregion

    //-Vytvorenie novej knihy

    /// <summary>
    /// calling complex function manipulating new book adding
    /// </summary>
    /// <returns>message for front end UI</returns>
    [Test]
    public async Task CallInsertBookTest()
    {
      string apiMap = "/insertbook"; //api address of function
      string result;
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
          //NUnit.Framework.Assert.That(HttpStatusCode.OK, Is.EqualTo(httpResponseMessage.StatusCode)); //not necessary ambigous
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
        string mes = $"An error occurred: {ex.Message}";
        Console.WriteLine(mes);
        NUnit.Framework.Assert.Fail(mes);
      }
    }
    //-Získanie detailov existujúcej knihy podľa ID
    [Test]
    public async Task GetBookInfo()
    {
      long bookid = bookInfoParam;
      Book? book;
      string mes;

      try
      {
        book = await _client.GetFromJsonAsync<Book>($"/getbookinfo/{bookid}");
        if (book != null)
          NUnit.Framework.Assert.That(book, Is.Not.Null);
        else
        {
          mes = "No data returned.";
          NUnit.Framework.Assert.Fail(mes);
        }
      }
      catch (Exception ex)
      {
        mes = $"An error occurred: {ex.Message}";
        Console.WriteLine(mes);
        NUnit.Framework.Assert.Fail(mes);
      }
    }

    //-Aktualizáciu existujúcej knihy
    [Test]
    public async Task UpdateBookInDb()
    {
      string apiMap = "/updbook"; //api address of function
      string result;
      HttpResponseMessage httpResponseMessage;
      try
      {
        // ID book value is required in Model object Book 
        Book? content = PrepareForBookUpdate(book1,bookUpd);
        if (content != null)
        {
          httpResponseMessage = await _client.PostAsJsonAsync(apiMap, content);

          if (httpResponseMessage.IsSuccessStatusCode)
          {
            result = await httpResponseMessage.Content.ReadAsStringAsync();
            NUnit.Framework.Assert.That(result, Is.Not.Null);
            Console.WriteLine($"Updated records: {result}");
          }
          else
          {
            string mes = $"Failed to insert book (Status Code: {httpResponseMessage.StatusCode})";
            NUnit.Framework.Assert.Fail(mes);
            Console.WriteLine(mes);
          }
        }
        else
        {
          string mes = $"No data for sending to API.";
          NUnit.Framework.Assert.Fail(mes);
          Console.WriteLine(mes);
        }
      }
      catch (Exception ex)
      {
        string mes = $"An error occurred: {ex.Message}";
        Console.WriteLine(mes);
        NUnit.Framework.Assert.Fail(mes);
      }
    }

    #region Private methods
    // simulation of reading and editing book record
    private Book? PrepareForBookUpdate(Book targetBook, Book sourceBook)
    {
      if (sourceBook == null || targetBook==null) return null;
      if (!(sourceBook.Id > 0)) 
        return null;
      else
      //Id is required in sourceBook
      targetBook.Id = sourceBook.Id;

      // Update properties if they have values in sourceBook

      if (!string.IsNullOrEmpty(sourceBook.Title))
        targetBook.Title = sourceBook.Title;

      if (!string.IsNullOrEmpty(sourceBook.Description))
        targetBook.Description = sourceBook.Description;

      if (sourceBook.Author_Id != null)
        targetBook.Author_Id = sourceBook.Author_Id;

      if (!string.IsNullOrEmpty(sourceBook.Publisher))
        targetBook.Publisher = sourceBook.Publisher;

      if (sourceBook.Edition_year != 0)
        targetBook.Edition_year = sourceBook.Edition_year;

      if (!string.IsNullOrEmpty(sourceBook.Isbn))
        targetBook.Isbn = sourceBook.Isbn;

      if (!string.IsNullOrEmpty(sourceBook.Ean_barcode))
        targetBook.Ean_barcode = sourceBook.Ean_barcode;

      if (sourceBook.Prints_num != 0)
        targetBook.Prints_num = sourceBook.Prints_num;

      if (!string.IsNullOrEmpty(sourceBook.Note))
        targetBook.Note = sourceBook.Note;

      return targetBook;
    }


    #endregion
  }
}
