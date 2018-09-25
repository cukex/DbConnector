using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Linq;

namespace DbConnector.Core
{
    public interface IDbConnectorSettings
    {
        /// <summary>
        /// Use to set the command type and override the default StoredProcedure type. (Optional)
        /// </summary>
        CommandType? CommandType { get; set; }

        string CommandText { get; set; }

        /// <summary>
        /// Use this property to override default ReadCommitted IsolationLevel. (Optional)
        /// </summary>
        IsolationLevel TransactionIsolationLevel { get; set; }

        IDbConnectorParameterCollection Parameters { get; set; }
    }


    public interface IDbConnectorParameter
    {
        ParameterDirection Direction { get; set; }
        bool IsNullable { get; set; }
        string ParameterName { get; set; }
        int Size { get; set; }
        string SourceColumn { get; set; }
        bool SourceColumnNullMapping { get; set; }
        object Value { get; set; }

        object DbTypeEnum
        {
            get;
        }

        void SetDbTypeEnum<TDbTypeEnum>(TDbTypeEnum dbType)
            where TDbTypeEnum : struct, IConvertible;
    }


    public interface IDbConnectorParameterCollection : IEnumerable<IDbConnectorParameter>
    {
        List<DbParameter> ToDbParameters<TDbCommand>(TDbCommand cmd) where TDbCommand : DbCommand;

        IDbConnectorParameter Add<TDbTypeEnum>(string parameterName, TDbTypeEnum dbType) where TDbTypeEnum : struct, IConvertible;

        IDbConnectorParameter Add<TDbTypeEnum>(string parameterName, TDbTypeEnum dbType, int size) where TDbTypeEnum : struct, IConvertible;

        IDbConnectorParameter Add<TDbTypeEnum>(string parameterName, TDbTypeEnum dbType, int size, string sourceColumn) where TDbTypeEnum : struct, IConvertible;

        IDbConnectorParameter AddWithValue(string parameterName, object value);
    }


    public interface IDbResult<T>
    {
        bool IsHasError
        {
            get;
        }

        Exception Error { get; set; }

        T Data { get; set; }
    }
}