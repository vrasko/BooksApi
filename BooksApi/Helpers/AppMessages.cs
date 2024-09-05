﻿namespace BooksApi.Helpers
{
  public static class AppMessages
  {
    public static readonly  Dictionary<string, string> MsgDict = new()
      {
      {"add_snm_auth","Uveďte priezvisko autora." },
      {"add_book_nm","Uveďte názov knihy." }

      };
  }
  //indicators for manipulating/managing messages
  public static class Consts
  {
    public const char chngIndicator = '*';
    public const char errorIndicator = '!';

  }
}
