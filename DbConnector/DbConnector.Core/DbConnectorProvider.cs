using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace DbConnector.Core
{
    public class DbConnectorProvider<TDbConnection>
         where TDbConnection : DbConnection
    {
        public static DbConnector<TDbConnection> Create(string connectionString)
        {
            return new DbConnector<TDbConnection>(connectionString);
        }
    }
}
