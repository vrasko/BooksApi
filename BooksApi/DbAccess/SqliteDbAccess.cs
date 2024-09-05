using Dapper;
using System.Data.SQLite;
using System.Data;

namespace BooksApi.DbAccess
{
  // see dapper examples on https://github.com/DapperLib/Dapper bottom of page - Readme.md
  public class SqliteDbAccess : IDisposable //, ISqliteDbAccess
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
