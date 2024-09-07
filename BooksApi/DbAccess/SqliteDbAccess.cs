using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace BooksApi.DbAccess
{
  // see dapper examples on https://github.com/DapperLib/Dapper bottom of page - Readme.md
  public class SqliteDbAccess : IDisposable, ISqliteDbAccess
  {
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private readonly IConfiguration _config;
    private bool isClosed = false;

    public SqliteDbAccess(IConfiguration config)
    {
      _config = config;
    }

    public string GetConnectionString(string name)
    {
      return _config.GetConnectionString(name) ?? "";
    }
    #region Dapper
    //returns 1 object
    public async Task<dynamic> LoadDataOneAnnonymAsync<U>(string sql, U parameters, string connectionName)
    {
      using IDbConnection cnn = new SqliteConnection(GetConnectionString(connectionName));
      return await cnn.QueryFirstOrDefaultAsync(sql, parameters);
      //return res;
    }
    public async Task<T?> LoadDataOneTypeAsync<T,U>(string sql, U parameters, string connectionName) //parameters different of retsult type
    {
      using IDbConnection cnn = new SqliteConnection(GetConnectionString(connectionName));
      return await cnn.QueryFirstOrDefaultAsync<T>(sql, parameters);
    }

    public async Task<List<T>> LoadListTypeAsync<T>(string sql, T? parameters, string connectionName)
    {
      using IDbConnection cnn = new SqliteConnection(GetConnectionString(connectionName));
      return (await cnn.QueryAsync<T>(sql, parameters)).ToList();
    }
    public async Task<dynamic?> ExecScalarAsync<U>(string sql, U parameters, string connectionName)
    {
      using IDbConnection cnn = new SqliteConnection(GetConnectionString(connectionName));
      return await cnn.ExecuteScalarAsync(sql, parameters);
    }
    public async Task<dynamic?> ExecAsync<U>(string sql, U parameters, string connectionName)
    {
      using IDbConnection cnn = new SqliteConnection(GetConnectionString(connectionName));
      return await cnn.ExecuteAsync(sql, parameters);
    }

    public async Task<dynamic> SaveDataInTransactionAsync<T>(string sql, T parameters)
    {
      return await _connection.ExecuteAsync(sql, parameters, transaction: _transaction, commandTimeout: 0);
    }
    public void StartTransaction(string connectionStringName)
    {
      string connectionString = GetConnectionString(connectionStringName);

      _connection = new SqliteConnection(connectionString);
      _connection.Open();

      _transaction = _connection.BeginTransaction();

      isClosed = false;
    }
    public void CommitTransaction()
    {
      _transaction?.Commit();
      _connection?.Close();

      isClosed = true;
    }
    public void RollbackTransaction()
    {
      _transaction?.Rollback();
      _connection?.Close();

      isClosed = true;
    }
    #endregion
    public void Dispose()
    {
      if (!isClosed)
      {
        try
        {
          CommitTransaction();
        }
        catch
        {
          //pridat serilog?
        }
      }
      _transaction = null;
      _connection = null;
      GC.SuppressFinalize(this);
    }
  }
}
