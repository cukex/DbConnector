using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace DbConnector.Core
{
    public interface IDbConnectorJob<T, TDbConnection>
        where TDbConnection : DbConnection
    {
        DbConnection CreateConnectionInstance();

        IDbResult<T> ExecuteContained();

        T Execute();

        Task<T> ExecuteAsync();

        Task<IDbResult<T>> ExecuteContainedAsync();
    }
}
