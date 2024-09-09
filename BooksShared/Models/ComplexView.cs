using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BooksShared.Models
{
  public class ComplexView
  {
    //table Customers
    public string? C_Name { get; set; }
    public string? C_Surname { get; set; }
    public string? C_Title { get; set; }
    public string? C_Ct_mail {  get; set; }
    //table Authors
    public string A_Name { get; set; } = "";
    public string A_Surname { get; set; } = "";
    //table Books
    public string B_Title { get; set; } = "";
    //table Loans
    public DateTime L_LoanDate { get; set; } = DateTime.Now; //Date of book lending
    public DateTime? L_RetDate { get; set; } //Date of book returning
    public DateTime L_DueDate { get; set; }
  }
}
