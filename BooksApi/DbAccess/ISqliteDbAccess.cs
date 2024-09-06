
namespace BooksApi.DbAccess
{
  public interface ISqliteDbAccess
  {
    Task<dynamic> LoadDataOneAnnonymAsync<U>(string sql, U parameters, string connectionName);
    void CommitTransaction();
    void Dispose();
    string GetConnectionString(string name);
    void RollbackTransaction();
    void StartTransaction(string connectionStringName);
    Task<dynamic> SaveDataInTransactionAsync<T>(string sql, T parameters);
  }
}