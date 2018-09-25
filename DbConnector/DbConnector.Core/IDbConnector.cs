using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace DbConnector.Core
{
    public interface IDbConnector<TDbConnection>
        where TDbConnection : DbConnection
    {
        DbConnectorJob<List<T>, TDbConnection> Read<T>(
           Action<IDbConnectorSettings> onInit,
           Func<List<T>, List<T>> onLoad = null,
           Func<Exception, IDbResult<List<T>>, IDbResult<List<T>>> onError = null) where T : new();

        DbConnectorJob<T, TDbConnection> ReadSingle<T>(
           Action<IDbConnectorSettings> onInit,
           Func<T, T> onLoad = null,
           Func<Exception, IDbResult<T>, IDbResult<T>> onError = null);

        DbConnectorJob<DataTable, TDbConnection> ReadToDataTable(
            Action<IDbConnectorSettings> onInit,
            Func<DataTable, DataTable> onLoad = null,
            Func<Exception, IDbResult<DataTable>, IDbResult<DataTable>> onError = null);

        DbConnectorJob<List<Dictionary<string, object>>, TDbConnection> ReadToDictionary(
            Action<IDbConnectorSettings> onInit,
            Func<List<Dictionary<string, object>>, List<Dictionary<string, object>>> onLoad = null,
            Func<Exception, IDbResult<List<Dictionary<string, object>>>, IDbResult<List<Dictionary<string, object>>>> onError = null,
            List<string> columnNamesToInclude = null,
            List<string> columnNamesToExclude = null);

        DbConnectorJob<T, TDbConnection> Read<T>(
            Action<IDbConnectorSettings> onInit,
            Func<DbDataReader, T> onLoad,
            CommandBehavior commandBehavior = CommandBehavior.Default,
            Func<Exception, IDbResult<T>, IDbResult<T>> onError = null);

        DbConnectorJob<bool, TDbConnection> NonQuery(
            Action<IDbConnectorSettings> onInit,
            Func<int, DbParameterCollection, bool, bool> onExecute = null,
            Func<Exception, IDbResult<bool>, IDbResult<bool>> onError = null);

        DbConnectorJob<T, TDbConnection> NonQuery<T>(
            Action<IDbConnectorSettings> onInit,
            Func<int, DbParameterCollection, T> onExecute,
            Func<Exception, IDbResult<T>, IDbResult<T>> onError = null);

        DbConnectorJob<bool, TDbConnection> NonQueries(
            Func<List<IDbConnectorSettings>> onInit,
            Action<int, DbParameterCollection> onExecuteOne = null,
            Func<Exception, IDbResult<bool>, IDbResult<bool>> onError = null);

        bool IsConnected();

        Task<bool> IsConnectedAsync();
    }
}
