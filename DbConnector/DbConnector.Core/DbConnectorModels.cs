using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Linq;

namespace DbConnector.Core
{
    public class DbConnectorSettings : IDbConnectorSettings
    {
        public DbConnectorSettings()
        {
            //Init param list
            this.Parameters = new DbConnectorParameterCollection();
            this.TransactionIsolationLevel = IsolationLevel.ReadCommitted;
        }

        /// <summary>
        /// Use to set the command type and override the default StoredProcedure type. (Optional)
        /// </summary>
        public CommandType? CommandType { get; set; }

        public string CommandText { get; set; }

        /// <summary>
        /// Use this property to override default ReadCommitted IsolationLevel. (Optional)
        /// </summary>
        public IsolationLevel TransactionIsolationLevel { get; set; }

        public IDbConnectorParameterCollection Parameters { get; set; }
    }


    public class DbConnectorParameter : IDbConnectorParameter
    {
        public ParameterDirection Direction { get; set; }
        public bool IsNullable { get; set; }
        public string ParameterName { get; set; }
        public int Size { get; set; }
        public string SourceColumn { get; set; }
        public bool SourceColumnNullMapping { get; set; }
        public object Value { get; set; }

        private object _dbTypeEnum;

        public object DbTypeEnum
        {
            get { return _dbTypeEnum; }
        }

        public void SetDbTypeEnum<TDbTypeEnum>(TDbTypeEnum dbType)
            where TDbTypeEnum : struct, IConvertible
        {
            if (!typeof(TDbTypeEnum).IsEnum)
            {
                throw new ArgumentException("TDbTypeEnum must be an enumerated type");
            }

            _dbTypeEnum = dbType;
        }
    }


    public class DbConnectorParameterCollection : List<DbConnectorParameter>, IDbConnectorParameterCollection
    {
        public List<DbParameter> ToDbParameters<TDbCommand>(TDbCommand cmd)
            where TDbCommand : DbCommand
        {
            List<DbParameter> toReturn = new List<DbParameter>();

            foreach (var item in this)
            {
                DbParameter toAdd = cmd.CreateParameter();
                toAdd.ParameterName = item.ParameterName;
                toAdd.Size = item.Size;
                toAdd.SourceColumn = item.SourceColumn;
                toAdd.Value = item.Value;
                toAdd.Direction = item.Direction;
                toAdd.SourceColumnNullMapping = item.SourceColumnNullMapping;

                var pInfoOfDbType = toAdd.GetType().GetProperties().FirstOrDefault(p => p.CanWrite && p.PropertyType == item.DbTypeEnum.GetType());

                if (pInfoOfDbType != null)
                {
                    pInfoOfDbType.SetValue(toAdd, item.DbTypeEnum);
                }

                toReturn.Add(toAdd);
            }

            return toReturn;
        }

        public IDbConnectorParameter Add<TDbTypeEnum>(string parameterName, TDbTypeEnum dbType)
            where TDbTypeEnum : struct, IConvertible
        {
            DbConnectorParameter toAdd = new DbConnectorParameter
            {
                ParameterName = parameterName
            };
            toAdd.SetDbTypeEnum<TDbTypeEnum>(dbType);

            this.Add(toAdd);

            return toAdd;
        }

        public IDbConnectorParameter Add<TDbTypeEnum>(string parameterName, TDbTypeEnum dbType, int size)
            where TDbTypeEnum : struct, IConvertible
        {
            DbConnectorParameter toAdd = new DbConnectorParameter
            {
                ParameterName = parameterName,
                Size = size
            };
            toAdd.SetDbTypeEnum<TDbTypeEnum>(dbType);

            this.Add(toAdd);

            return toAdd;
        }

        public IDbConnectorParameter Add<TDbTypeEnum>(string parameterName, TDbTypeEnum dbType, int size, string sourceColumn)
            where TDbTypeEnum : struct, IConvertible
        {
            DbConnectorParameter toAdd = new DbConnectorParameter
            {
                ParameterName = parameterName,
                Size = size,
                SourceColumn = sourceColumn
            };
            toAdd.SetDbTypeEnum<TDbTypeEnum>(dbType);

            this.Add(toAdd);

            return toAdd;
        }

        public IDbConnectorParameter AddWithValue(string parameterName, object value)
        {
            DbConnectorParameter toAdd = new DbConnectorParameter
            {
                ParameterName = parameterName,
                Value = value
            };

            this.Add(toAdd);

            return toAdd;
        }

        IEnumerator<IDbConnectorParameter> IEnumerable<IDbConnectorParameter>.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


    public class DbResult<T> : IDbResult<T>
    {
        public bool IsHasError
        {
            get
            {
                return this.Error != null;
            }
        }

        public Exception Error { get; set; }

        public T Data { get; set; }
    }
}