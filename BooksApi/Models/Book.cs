
using System.ComponentModel.DataAnnotations;
using BooksApi.Helpers;

namespace BooksApi.Models
{
  public class Book
  {
    //for simplification let every book has only one author then relation table Books<->Authors is not required
    [Key]
    public long Id { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "add_book_nm")]// "Uveďte názov knihy." need to resolve in method  localization app
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    [Required]
    public long Author_Id { get; set; }
    public string? Publisher { get; set; }
    [YearRange(1500)]
    public int Edition_year { get; set; } // year of publication with example of custom validation
    public string? Isbn { get; set; } //international standard code of book
    public string? Ean_barcode { get; set; } //barcode 
    [Required]
    public int Prints_num { get; set; } = 0; //number of prints in library, default 0
    public string? Note { get; set; } //aby other tags and notes for library admin

  }

  public class YearRangeAttribute : ValidationAttribute

  //due to compare dynamic value there must be used custom validation attribute
  {
    private readonly int _minYear;

    public YearRangeAttribute(int minYear)
    {
      _minYear = minYear;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
      if (value is int year)
      {
        int currentYear = DateTime.Now.Year;
        if (year >= _minYear && year <= currentYear)
        {
          return ValidationResult.Success;
        }
        else
        {
          return new ValidationResult($"Rok vydania musí byť medzi {_minYear} a {currentYear}.");
        }
      }
      return new ValidationResult("Chybný formát pre rok vydania.");
    }
  }
}
