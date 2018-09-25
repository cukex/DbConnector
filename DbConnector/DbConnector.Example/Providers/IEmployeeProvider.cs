using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using DbConnector.Example.Entities;
using DbConnector.Example.Providers;

namespace DbConnector.Core.Providers
{
    public interface IEmployeeProvider<TDbConnection> : IEntityProvider<Employee>
        where TDbConnection : DbConnection
    {
        Task<List<Employee>> GetAllSimple();
        Task<Employee> GetSingle(int id);
        Task<DataTable> GetByDataTable();
    }
}
