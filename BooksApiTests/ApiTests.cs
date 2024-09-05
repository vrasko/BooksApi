//using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using BooksApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Net;
using BooksApi.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BooksApi.Tests
{
  public class ApiTests
  {
    #region Setup
    private readonly HttpClient _client = new();
    private string _messPosted = "";
    private const string apiUrlBase = "https://localhost:7262";

    private Author author1 = new()
    {
      Name = "Stephen",
      Surname = "King",
      Description = "master of horror"
    };

    private Book book1 = new()
    {
      Title = "Misery",
      Description = "Horror",
      //Author_Id: ,
      Publisher = "Viking Press",
      Edition_year = 1987,
      Isbn = "0670813648",
      Ean_barcode = "9780450417399",
      Prints_num = 11,
      Note = "English original publishing"
    };

    #endregion


    [Test]
    public async Task<string> CallInsertBookTest() //vytvori novy zaznam
    {
      string apiMap = apiUrlBase + "/insertbook"; //api addres of function
      string result = "";
      HttpResponseMessage httpResponseMessage;
      //}

      try
      {
        //// alt1
        // MultipartFormDataContent multiContent = new()
        // {
        //   { JsonContent.Create(author1), "author" },
        //   { JsonContent.Create(book1),"book" }

        //  }; 
       // httpResponseMessage = await _client.PostAsync(apiMap, multiContent);

        //alt2 
        var multiContent = new {Authors = author1,Books = book1};
        httpResponseMessage = await _client.PostAsJsonAsync(apiMap, multiContent);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
          result = await httpResponseMessage.Content.ReadAsStringAsync();
          //return result; //sprava o zapisanych riadkoch z DB procedury
        }
        result += string.Format("Nepodarilo sa zapísať údaje (Status Code: {0})\n", httpResponseMessage.StatusCode);
        return result;
      }
      catch (Exception ex)
      {
        result += string.Format("Nastala chyba: {0}\n", ex.Message);
      }
      return result;

    }

  }