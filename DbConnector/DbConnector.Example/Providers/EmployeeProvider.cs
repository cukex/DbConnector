using DbConnector.Example.Entities;
using DbConnector.Example.Providers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;


namespace DbConnector.Core.Providers
{
    /// <summary>
    /// This class architecture allows for the querying of any type of database.
    /// </summary>
    /// <typeparam name="TDbConnection"></typeparam>
    public class EmployeeProvider<TDbConnection>
        : EntityProvider<TDbConnection, Employee>, IEmployeeProvider<TDbConnection>
        where TDbConnection : DbConnection
    {
        public EmployeeProvider(IDbConnector<TDbConnection> dbConnector)
            : base(dbConnector)
        {

        }

        public override Task<IDbResult<List<Employee>>> GetAll()
        {
            return _dbConnector.Read<Employee>(
               onInit: (settings) =>
               {
                   settings.CommandType = System.Data.CommandType.Text;
                   settings.CommandText = "SELECT * FROM Employees";

               }).ExecuteContainedAsync();
        }

        public Task<List<Employee>> GetAllSimple()
        {
            return _dbConnector.Read<Employee>(
               onInit: (settings) =>
               {
                   settings.CommandType = System.Data.CommandType.Text;
                   settings.CommandText = "SELECT * FROM Employees";

               }).ExecuteAsync();
        }

        public Task<Employee> GetSingle(int id)
        {
            return _dbConnector.ReadSingle<Employee>(
               onInit: (settings) =>
               {
                   settings.CommandType = System.Data.CommandType.StoredProcedure;
                   settings.CommandText = "dbo.GetEmployee";

                   settings.Parameters.AddWithValue("id", id);

               }).ExecuteAsync();
        }

        public Task<DataTable> GetByDataTable()
        {
            return _dbConnector.ReadToDataTable(
               onInit: (settings) =>
               {
                   settings.CommandType = System.Data.CommandType.StoredProcedure;
                   settings.CommandText = "SELECT * FROM Employees";

               }).ExecuteAsync();
        }
    }
}
