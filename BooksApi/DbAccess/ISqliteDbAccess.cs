

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
    Task<T?> LoadDataOneTypeAsync<T,U>(string sql, U parameters, string connectionName);
    Task<List<T>> LoadListTypeAsync<T>(string sql, T? parameters, string connectionName);
    Task<dynamic?> ExecScalarAsync<U>(string sql, U parameters, string connectionName);
    Task<dynamic?> ExecAsync<U>(string sql, U parameters, string connectionName);
  }
}