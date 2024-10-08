﻿using System.ComponentModel.DataAnnotations;
namespace BooksShared.Models
{
  public class Customer
  {
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Title { get; set; }
    public string? Ad_street { get; set; }
    public string? Ad_number { get; set; }
    public string? Ad_city { get; set; }
    public string? Ad_zip { get; set; } //PSC
    public string? Ad_country { get; set; }
    public string? Ct_phone { get; set; }
    [EmailAddress]
    public string? Ct_mail { get; set; }
    public string? Note { get;set; } //any other tags or notes for library admin
  }
}
