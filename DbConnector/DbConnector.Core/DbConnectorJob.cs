using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace DbConnector.Core
{
    public abstract class DbConnectorJob<T, TDbConnection> : IDbConnectorJob<T, TDbConnection>
        where TDbConnection : DbConnection
    {
        #region Properties

        protected Func<IDbResult<T>> _onInit;
        protected Func<List<IDbConnectorSettings>> _onSetCommand;
        protected Func<DbCommand, T> _onOpen;
        protected Func<Exception, IDbResult<T>, IDbResult<T>> _onError;
        protected string _connectionString;

        public Func<IDbResult<T>> OnInit
        {
            get { return _onInit; }
        }

        public Func<List<IDbConnectorSettings>> OnSetCommand
        {
            get { return _onSetCommand; }
        }

        public Func<DbCommand, T> OnOpen
        {
            get { return _onOpen; }
        }

        public Func<Exception, IDbResult<T>, IDbResult<T>> OnError
        {
            get { return _onError; }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        #endregion


        public DbConnectorJob(
            string connectionString,
            Func<IDbResult<T>> onInit,
            Func<List<IDbConnectorSettings>> onSetCommand,
            Func<DbCommand, T> onOpen,
            Func<Exception, IDbResult<T>, IDbResult<T>> onError)
        {
            _connectionString = connectionString;
            _onInit = onInit;
            _onSetCommand = onSetCommand;
            _onOpen = onOpen;
            _onError = onError;
        }

        public virtual DbConnection CreateConnectionInstance()
        {
            return Activator.CreateInstance<TDbConnection>();
        }

        public virtual IDbResult<T> ExecuteContained()
        {
            IDbResult<T> result = new DbResult<T>();

            if (typeof(T).IsClass)
            {
                result.Data = Activator.CreateInstance<T>();
            }
            else
            {
                result.Data = default(T);
            }


            if (_onInit != null)
            {
                result = _onInit();
            }

            try
            {
                using (var conn = this.CreateConnectionInstance())
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();


                    var cmdModelItems =
                           _onSetCommand != null
                           ? _onSetCommand() : new List<IDbConnectorSettings> { new DbConnectorSettings() };


                    using (var transaction = conn.BeginTransaction(cmdModelItems.First().TransactionIsolationLevel))
                    {
                        try
                        {
                            foreach (var cmdModel in cmdModelItems)
                            {
                                using (var cmd = conn.CreateCommand())
                                {
                                    cmd.Connection = conn;
                                    cmd.CommandType = cmdModel.CommandType ?? System.Data.CommandType.StoredProcedure;
                                    cmd.CommandText = cmdModel.CommandText;


                                    if (cmdModel.Parameters != null && cmdModel.Parameters.Any())
                                        cmd.Parameters.AddRange(cmdModel.Parameters.ToDbParameters(cmd).ToArray());


                                    if (_onOpen != null)
                                    {
                                        result.Data = _onOpen(cmd);
                                    }
                                }
                            }

                            transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            throw e;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Log();
                result.Error = ex;

                if (_onError != null)
                {
                    result = _onError(ex, result);
                }
            }

            return result;
        }

        public virtual T Execute()
        {
            return this.ExecuteContained().Data;
        }

        public virtual Task<T> ExecuteAsync()
        {
            return Task.Run<T>(() => Execute());
        }

        public virtual Task<IDbResult<T>> ExecuteContainedAsync()
        {
            return Task.Run<IDbResult<T>>(() => ExecuteContained());
        }
    }

    public class DbConnectorJobTask<T, TDbConnection> : DbConnectorJob<T, TDbConnection>
        where TDbConnection : DbConnection
    {
        internal DbConnectorJobTask(string connectionString,
            Func<IDbResult<T>> onInit,
            Func<List<IDbConnectorSettings>> onSetCommand,
            Func<DbCommand, T> onOpen,
            Func<Exception, IDbResult<T>,
                IDbResult<T>> onError)
            : base(connectionString, onInit, onSetCommand, onOpen, onError)
        {

        }

        public static IDbResult<bool> ExecuteAll(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            params DbConnectorJob<T, TDbConnection>[] taskItems)
        {
            IDbResult<bool> result = new DbResult<bool>();

            try
            {
                if (taskItems != null && taskItems.Length > 0)
                {
                    string connectionString = taskItems[0].ConnectionString;

                    using (var conn = taskItems[0].CreateConnectionInstance())
                    {
                        conn.ConnectionString = connectionString;
                        conn.Open();

                        using (var transaction = conn.BeginTransaction(isolationLevel))
                        {
                            try
                            {
                                foreach (var item in taskItems)
                                {
                                    var cmdModelItems = item.OnSetCommand != null
                                                                ? item.OnSetCommand() : new List<IDbConnectorSettings> { new DbConnectorSettings() };

                                    foreach (var cmdModel in cmdModelItems)
                                    {
                                        using (var cmd = conn.CreateCommand())
                                        {
                                            cmd.Connection = conn;
                                            cmd.CommandType = cmdModel.CommandType ?? System.Data.CommandType.StoredProcedure;
                                            cmd.CommandText = cmdModel.CommandText;

                                            if (cmdModel.Parameters != null && cmdModel.Parameters.Any())
                                                cmd.Parameters.AddRange(cmdModel.Parameters.ToDbParameters(cmd).ToArray());

                                            if (item.OnOpen != null)
                                            {
                                                item.OnOpen(cmd);
                                            }
                                        }
                                    }
                                }

                                transaction.Commit();

                                result.Data = true;
                            }
                            catch (Exception e)
                            {
                                transaction.Rollback();
                                throw e;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Log();
                result.Error = ex;
            }

            return result;
        }

        public static Task<IDbResult<bool>> ExecuteAllAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            params DbConnectorJob<T, TDbConnection>[] taskItems)
        {
            return Task.Run<IDbResult<bool>>(() => ExecuteAll(isolationLevel, taskItems));
        }
    }
}
