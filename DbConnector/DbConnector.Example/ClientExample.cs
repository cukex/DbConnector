using DbConnector.Core;
using DbConnector.Core.Providers;
using DbConnector.Example.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace DbConnector.Example
{
    public class ClientExample
    {
        IEmployeeProvider<SqlConnection> _pdrEmployee;

        public ClientExample()
        {
            //TODO: Potentially use dependency injection to do the following:

            //Also, note that you can use any type of data provider adapter that implements a DbConnection.
            //E.g. PostgreSQL, Oracle, MySql, SQL Server

            //Example using SQL Server connection
            IDbConnector<SqlConnection> dbConnector = new DbConnector<SqlConnection>("connection string goes here");

            _pdrEmployee = new EmployeeProvider<SqlConnection>(dbConnector);
        }

        public async Task<Employee> GetEmployee(int id)
        {
            //The use of Task/Async allows us to make multiple asynchronous calls
            //to the database leveraging the Task.WaitAll architecture

            return await _pdrEmployee.GetSingle(id);
        }
    }
}
