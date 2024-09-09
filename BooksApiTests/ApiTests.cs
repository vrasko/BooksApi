using BooksShared.Models;
using Microsoft.AspNetCore.Routing.Template;
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
    private readonly long bookToDel = 2;

    private readonly Loan loan1 = new Loan() //new Loan, nova zapozicka
    {
      Book_Id = 1,
      Cust_Id = 1,
      LoanDate = DateTime.Now.AddMonths(-1),
      DueDate = DateTime.Now.AddMonths(1)
    };

    private readonly long bookReturnParam = 1;

    #endregion
    //*********************************************
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
    //*********************************************
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

    //*********************************************
    //-Aktualizáciu existujúcej knihy - Update book
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

    //*********************************************
    //-Odstránenie knihy - Delete book
    [Test]
    public async Task DeleteBook()
    {
      // Id book required as parameter for this example
      long bookid = bookToDel;
      int? recordsDeleted;
      string mes;

      try
      {
        recordsDeleted = await _client.GetFromJsonAsync<int>($"/deletebook/{bookid}");
        if (recordsDeleted != null)
          NUnit.Framework.Assert.That(recordsDeleted, Is.EqualTo(1));
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

    //*********************************************
    //- Vytvorenie novej zápožičky - New loan
    [Test]
    public async Task AddNewLoan()
    {
      string apiMap = "/newloan"; //api address of function
      string result;
      HttpResponseMessage httpResponseMessage;
      try
      {
        // ID book and customer are required in Model object Loan - prepared by Front end  
        Loan? content = loan1;
        if (content != null)
        {
          httpResponseMessage = await _client.PostAsJsonAsync(apiMap, content);

          if (httpResponseMessage.IsSuccessStatusCode)
          {
            result = await httpResponseMessage.Content.ReadAsStringAsync();
            NUnit.Framework.Assert.That(result, Is.EqualTo("1"));
            Console.WriteLine($"Inserted records: {result}");
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
    //*********************************************
    //- Potvrdenie o vrátení vypožičanej knihy - Confirmation of the return of the borrowed book
    /// <summary>
    /// returns necessary data for creating the confirmation of return via some template
    /// </summary>
    /// <returns>complex view for confirmation</returns>
    [Test]
    public async Task GetReturnConfirm()
    {
      //Id of book required (prepared by UI form)
      long bookid = bookReturnParam;
      ComplexView? cw;
      string mes;

      try
      {
        cw = await _client.GetFromJsonAsync<ComplexView>($"/getretconf/{bookid}");
        if (cw != null)
        {
          NUnit.Framework.Assert.That(cw, Is.Not.Null);
          //example text of confirmation
          string confirmation=GetConfirmation(cw);
          NUnit.Framework.Assert.That(confirmation, Is.Not.Empty);
          Console.WriteLine(confirmation);
        }
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
    //*********************************************
    //- Pripomienka na vratenie knihy den pred dátumom vrátenia - Reminder of the return of the borrowed book
    //- Služba sa bude spúšťať každý deň cez Windows Task Scheduler - Service runs every day via Windows Task scheduler
    /// <summary>
    /// sends emails for cusomers whose return deta of Loans is tomorrow
    /// </summary>
    /// <returns>message</returns>
    [Test]
    public async Task SendReminders()
    {
      string? res;
      string mes;

      try
      {
        res = await _client.GetStringAsync($"/sendreminder");
        if (res != null)
        {
          NUnit.Framework.Assert.That(res, Is.Not.Empty);
           Console.WriteLine(res);
        }
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


    //**************************************************
    #region Private methods
    // simulation of reading and editing book record
    private static Book? PrepareForBookUpdate(Book targetBook, Book sourceBook)
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
    private static string GetConfirmation(ComplexView cw)
    {
      try
      {
        string conf = $"Potvrdzujeme, že p. {cw.C_Title} {cw.C_Name} {cw.C_Surname} vrátil dňa : {DateOnly.FromDateTime((DateTime)cw.L_RetDate)} knihu {cw.A_Name} {cw.A_Surname}: {cw.B_Title}, zapožičanú dňa: {DateOnly.FromDateTime(cw.L_LoanDate)}.\r\nĎakujeme, že ste s nami.";
        return conf;
      }
      catch (Exception)
      {
        return "";
      }
    }

    #endregion
  }
}
