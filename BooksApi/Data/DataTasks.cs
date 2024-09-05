using BooksApi.Models;
namespace BooksApi.Data
{
  public class DataTasks : IDataTasks
  {
    private readonly IConfiguration _config;
    private readonly string _connName = "BooksApiSQLite";
    public DataTasks(IConfiguration config)
    {
      _config = config;
    }
    public async Task<dynamic> InsNewBook(Author author, Book book) //parametre json
    {
      //this method is for inserting a new book. The target database resolves its procedure if the book is in database. if it is, DB procedure increments only number of prints. If there is no print of the book, the new record will be added. 
      //It is not efficient resolve the process of testing existence of book in api better practise is DB procedure.
      string sql;
      long? idAut=null;
      sql = "SELECT author_id FROM Books  WHERE UPPER(title) =:Title AND ean_barcode = :Ean_barcode"; //'MARíNA', 9999000083137
      return _postgres.LoadDataOneAnnonymAsync<dynamic>(sql, new { Title = book.Title, Ean_barcode=book.Ean_barcode }, _connName);

      if (author.Id == null) //insert author
      {
         sql = "INSERT into  gp.o_osoba (typ_osoby, nazov, prav_forma_id, id_os, dic, ic_dph, meno, priezvisko, tit_pred, tit_za, iban, ad_ulica, ad_cislo, ad_obec, ad_psc, ad_stat, ct_email, ct_phone, last_change, tid, usr_id) VALUES(:Typ_osoby,:Nazov, :Prav_forma_id, :Id_os,:Dic, :Ic_dph, :Meno, :Priezvisko, :Tit_pred, :Tit_za, :Iban, :Ad_ulica, :Ad_cislo, :Ad_obec, :Ad_psc,:Ad_stat,:Ct_email, :Ct_phone, now(), :Tid, :Usr_id )";
       
      }
      //zalozit autora a vratit jeho ID
      book.Author_Id = idAut;
      //zalozit knihu



      _postgres.StartTransaction(_connName);
      var res = await _postgres.SaveDataInTransactionAsync(sql, osoba);
      _postgres.CommitTransaction();
      return res;
    }
  }
}
