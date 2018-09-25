using DbConnector.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace DbConnector.Core
{
    public class DbConnector<TDbConnection> : IDbConnector<TDbConnection>
        where TDbConnection : DbConnection
    {
        #region Main

        private string _connectionString;

        public DbConnector(string connectionString)
        {
            _connectionString = connectionString;
        }

        #endregion

        #region Implementation

        public DbConnectorJob<List<T>, TDbConnection> Read<T>(
            Action<IDbConnectorSettings> onInit,
            Func<List<T>, List<T>> onLoad = null,
            Func<Exception, IDbResult<List<T>>, IDbResult<List<T>>> onError = null)
            where T : new()
        {
            return new DbConnectorJobTask<List<T>, TDbConnection>
                (
                    connectionString: _connectionString,
                    onInit: () =>
                    {
                        return new DbResult<List<T>>
                        {
                            Data = new List<T>()
                        };
                    },
                    onSetCommand: () =>
                    {
                        var cmdModel = new DbConnectorSettings();

                        onInit(cmdModel);

                        return new List<IDbConnectorSettings>() { cmdModel };
                    },
                    onOpen: (cmd) =>
                    {
                        using (var odr = cmd.ExecuteReader())
                        {
                            var data = odr.ToList<T>();

                            if (onLoad != null)
                            {
                                data = onLoad(data);
                            }

                            return data;
                        }
                    },
                    onError: (ex, result) =>
                    {
                        if (onError != null)
                        {
                            result = onError(ex, result);
                        }

                        return result;
                    }
                );
        }

        public DbConnectorJob<T, TDbConnection> ReadSingle<T>(
            Action<IDbConnectorSettings> onInit,
            Func<T, T> onLoad = null,
            Func<Exception, IDbResult<T>, IDbResult<T>> onError = null)
        {
            return new DbConnectorJobTask<T, TDbConnection>
                (
                    connectionString: _connectionString,
                    onInit: () =>
                    {
                        if (typeof(T).IsClass)
                        {
                            return new DbResult<T>
                            {
                                Data = Activator.CreateInstance<T>()
                            };
                        }
                        else
                        {
                            return new DbResult<T>
                            {
                                Data = default(T)
                            };
                        }
                    },
                    onSetCommand: () =>
                    {
                        var cmdModel = new DbConnectorSettings();

                        onInit(cmdModel);

                        return new List<IDbConnectorSettings>() { cmdModel };
                    },
                    onOpen: (cmd) =>
                    {
                        using (var odr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            T data = odr.ToSingle<T>();

                            if (onLoad != null)
                            {
                                data = onLoad(data);
                            }

                            return data;
                        }
                    },
                    onError: (ex, result) =>
                    {
                        if (onError != null)
                        {
                            result = onError(ex, result);
                        }

                        return result;
                    }
                );
        }

        public DbConnectorJob<DataTable, TDbConnection> ReadToDataTable(
            Action<IDbConnectorSettings> onInit,
            Func<DataTable, DataTable> onLoad = null,
            Func<Exception, IDbResult<DataTable>, IDbResult<DataTable>> onError = null)
        {
            return new DbConnectorJobTask<DataTable, TDbConnection>
                (
                    connectionString: _connectionString,
                    onInit: () =>
                    {
                        return new DbResult<DataTable>
                        {
                            Data = new DataTable()
                        };
                    },
                    onSetCommand: () =>
                    {
                        var cmdModel = new DbConnectorSettings();

                        onInit(cmdModel);

                        return new List<IDbConnectorSettings>() { cmdModel };
                    },
                    onOpen: (cmd) =>
                    {
                        using (var odr = cmd.ExecuteReader())
                        {
                            var data = odr.ToDataTable();

                            if (onLoad != null)
                            {
                                data = onLoad(data);
                            }

                            return data;
                        }
                    },
                    onError: (ex, result) =>
                    {
                        if (onError != null)
                        {
                            result = onError(ex, result);
                        }

                        return result;
                    }
                );
        }

        public DbConnectorJob<List<Dictionary<string, object>>, TDbConnection> ReadToDictionary(
            Action<IDbConnectorSettings> onInit,
            Func<List<Dictionary<string, object>>, List<Dictionary<string, object>>> onLoad = null,
            Func<Exception, IDbResult<List<Dictionary<string, object>>>, IDbResult<List<Dictionary<string, object>>>> onError = null,
            List<string> columnNamesToInclude = null,
            List<string> columnNamesToExclude = null)
        {
            return new DbConnectorJobTask<List<Dictionary<string, object>>, TDbConnection>
                (
                    connectionString: _connectionString,
                    onInit: () =>
                    {
                        return new DbResult<List<Dictionary<string, object>>>
                        {
                            Data = new List<Dictionary<string, object>>()
                        };
                    },
                    onSetCommand: () =>
                    {
                        var cmdModel = new DbConnectorSettings();

                        onInit(cmdModel);

                        return new List<IDbConnectorSettings>() { cmdModel };
                    },
                    onOpen: (cmd) =>
                    {
                        using (var odr = cmd.ExecuteReader())
                        {
                            var data = odr.ToListDictionary(columnNamesToInclude, columnNamesToExclude);

                            if (onLoad != null)
                            {
                                data = onLoad(data);
                            }

                            return data;
                        }
                    },
                    onError: (ex, result) =>
                    {
                        if (onError != null)
                        {
                            result = onError(ex, result);
                        }

                        return result;
                    }
                );
        }

        public DbConnectorJob<T, TDbConnection> Read<T>(
            Action<IDbConnectorSettings> onInit,
            Func<DbDataReader, T> onLoad,
            CommandBehavior commandBehavior = CommandBehavior.Default,
            Func<Exception, IDbResult<T>, IDbResult<T>> onError = null)
        {
            return new DbConnectorJobTask<T, TDbConnection>
                (
                    connectionString: _connectionString,
                    onInit: () =>
                    {
                        return new DbResult<T>
                        {
                            Data = default(T)
                        };
                    },
                    onSetCommand: () =>
                    {
                        var cmdModel = new DbConnectorSettings();

                        onInit(cmdModel);

                        return new List<IDbConnectorSettings>() { cmdModel };
                    },
                    onOpen: (cmd) =>
                    {
                        using (var odr = cmd.ExecuteReader(commandBehavior))
                        {
                            return onLoad(odr);
                        }
                    },
                    onError: (ex, result) =>
                    {
                        if (onError != null)
                        {
                            result = onError(ex, result);
                        }

                        return result;
                    }
                );
        }

        public DbConnectorJob<bool, TDbConnection> NonQuery(
            Action<IDbConnectorSettings> onInit,
            Func<int, DbParameterCollection, bool, bool> onExecute = null,
            Func<Exception, IDbResult<bool>, IDbResult<bool>> onError = null)
        {
            return new DbConnectorJobTask<bool, TDbConnection>
                (
                    connectionString: _connectionString,
                    onInit: () =>
                    {
                        return new DbResult<bool>();
                    },
                    onSetCommand: () =>
                    {
                        var cmdModel = new DbConnectorSettings();

                        onInit(cmdModel);

                        return new List<IDbConnectorSettings>() { cmdModel };
                    },
                    onOpen: (cmd) =>
                    {
                        int numberOfRowsAffected = cmd.ExecuteNonQuery();

                        if (onExecute != null)
                        {
                            return onExecute(numberOfRowsAffected, cmd.Parameters, true);
                        }

                        return true;
                    },
                    onError: (ex, result) =>
                    {
                        if (onError != null)
                        {
                            result = onError(ex, result);
                        }

                        return result;
                    }
                );
        }

        public DbConnectorJob<T, TDbConnection> NonQuery<T>(
            Action<IDbConnectorSettings> onInit,
            Func<int, DbParameterCollection, T> onExecute,
            Func<Exception, IDbResult<T>, IDbResult<T>> onError = null)
        {
            return new DbConnectorJobTask<T, TDbConnection>
                (
                    connectionString: _connectionString,
                    onInit: () =>
                    {
                        if (typeof(T).IsClass)
                        {
                            return new DbResult<T>
                            {
                                Data = Activator.CreateInstance<T>()
                            };
                        }
                        else
                        {
                            return new DbResult<T>
                            {
                                Data = default(T)
                            };
                        }
                    },
                    onSetCommand: () =>
                    {
                        var cmdModel = new DbConnectorSettings();

                        onInit(cmdModel);

                        return new List<IDbConnectorSettings>() { cmdModel };
                    },
                    onOpen: (cmd) =>
                    {
                        int numberOfRowsAffected = cmd.ExecuteNonQuery();

                        return onExecute(numberOfRowsAffected, cmd.Parameters);
                    },
                    onError: (ex, result) =>
                    {
                        if (onError != null)
                        {
                            result = onError(ex, result);
                        }

                        return result;
                    }
                );
        }

        public DbConnectorJob<bool, TDbConnection> NonQueries(
            Func<List<IDbConnectorSettings>> onInit,
            Action<int, DbParameterCollection> onExecuteOne = null,
            Func<Exception, IDbResult<bool>, IDbResult<bool>> onError = null)
        {
            return new DbConnectorJobTask<bool, TDbConnection>
                (
                    connectionString: _connectionString,
                    onInit: () =>
                    {
                        return new DbResult<bool>();
                    },
                    onSetCommand: () =>
                    {
                        return onInit();
                    },
                    onOpen: (cmd) =>
                    {
                        int numberOfRowsAffected = cmd.ExecuteNonQuery();
                        onExecuteOne(numberOfRowsAffected, cmd.Parameters);
                        return true;
                    },
                    onError: (ex, result) =>
                    {
                        result.Data = false;

                        if (onError != null)
                        {
                            result = onError(ex, result);
                        }

                        return result;
                    }
                );
        }

        public bool IsConnected()
        {
            bool isSuccess = false;

            try
            {
                using (var conn = Activator.CreateInstance<TDbConnection>())
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();
                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                ex.Log();
                isSuccess = false;
            }


            return isSuccess;
        }

        public Task<bool> IsConnectedAsync()
        {
            return Task.Run<bool>(() => IsConnected());
        }

        #endregion
    }
}
