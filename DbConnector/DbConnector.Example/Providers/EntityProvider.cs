using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using DbConnector.Core;
using DbConnector.Core.Extensions;

namespace DbConnector.Example.Providers
{
    public abstract class EntityProvider<TDbConnection, T> : IEntityProvider<T>
        where T : new()
        where TDbConnection : DbConnection
    {
        protected static IDbConnector<TDbConnection> _dbConnector;

        public EntityProvider(IDbConnector<TDbConnection> dbConnector)
        {
            _dbConnector = dbConnector;
        }

        public virtual Task<IDbResult<List<T>>> GetAll()
        {
            return _dbConnector.Read<T>(
               onInit: (settings) =>
               {
                   settings.CommandType = System.Data.CommandType.Text;
                   settings.CommandText = "Select * from " + typeof(T).GetAttributeValue((TableAttribute ta) => ta.Name) ?? typeof(T).Name;

               }).ExecuteContainedAsync();
        }

        public virtual Task<IDbResult<T>> Get()
        {
            return _dbConnector.ReadSingle<T>(
               onInit: (settings) =>
               {
                   settings.CommandType = System.Data.CommandType.Text;
                   settings.CommandText = "Select * from "
                   + typeof(T).GetAttributeValue((TableAttribute ta) => ta.Name) ?? typeof(T).Name
                   + " fetch first 1 rows only";

               }).ExecuteContainedAsync();
        }
    }
}
