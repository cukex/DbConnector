using DbConnector.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace DbConnector.Example.Providers
{
    public interface IEntityProvider<T> where T : new()
    {
        Task<IDbResult<List<T>>> GetAll();
        Task<IDbResult<T>> Get();
    }
}
