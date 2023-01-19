using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using DashboardWebApp.DataTypes;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DashboardWebApp.Data
{
    public class DataAccessLayer : IDataAccessLayer
    {
        private readonly string connectionString;
        public DataAccessLayer(IDistributedCache cache, ILogger<DataAccessLayer> logger, IConfiguration configuration)
        {
            this.m_commandTimeout = 30;
            this.m_commandHash = Hashtable.Synchronized(this.m_commandHash);
            this.m_dataCacheItems = Hashtable.Synchronized(this.m_dataCacheItems);

            this.cache = cache;
            this.logger = logger;

            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public int CommandTimeout
        {
            get
            {
                return this.m_commandTimeout;
            }
            set
            {
                this.m_commandTimeout = value;
            }
        }

        public int ExecuteCommand(IDbConnection conIn, string procedureName, bool processOutput, params object[] parameters)
        {
            try
            {
                IDbCommand dbCommand = this.BuildCommand(conIn, procedureName, parameters);

                int result = dbCommand.ExecuteNonQuery();
                if (processOutput)
                {
                    this.ProcessOutputParams(dbCommand, parameters);
                }
                return result;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Error occured. ProcedureName:{procedureName},Parameters:{parameters}");
                throw;
            }
        }

        public int ExecuteCommand(string procedureName, bool processOutput, params object[] parameters)
        {
            IDbConnection connection = DataClassFactory.GetConnection(connectionString);
            int result;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                result = this.ExecuteCommand(connection, procedureName, processOutput, parameters);
                connection.Close();
            }
            else
            {
                result = this.ExecuteCommand(connection, procedureName, processOutput, parameters);
            }
            return result;
        }

        public IDataReader GetDataReader(IDbConnection conIn, string SQL)
        {
            try
            {
                IDbCommand dbCommand = conIn.CreateCommand();
                dbCommand.CommandText = SQL;
                dbCommand.CommandTimeout = this.m_commandTimeout;
                TransactionManager currentTransactionManager = TransactionManager.GetCurrentTransactionManager();
                if (currentTransactionManager == null || !currentTransactionManager.IsSimpleTransaction || !TransactionManager.DalManagedTransactions)
                {
                    return dbCommand.ExecuteReader(CommandBehavior.CloseConnection);
                }
                return dbCommand.ExecuteReader();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Error occured. SQL:{SQL}");
                throw;
            }
        }

        public IDataReader GetDataReader(string connectionString, string SQL)
        {
            IDbConnection connection = DataClassFactory.GetConnection(connectionString);
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return this.GetDataReader(connection, SQL);
        }

        public IDataReader GetDataReader(IDbConnection conIn, string procedureName, bool processOutput, params object[] parameters)
        {
            try
            {
                IDbCommand dbCommand = this.BuildCommand(conIn, procedureName, parameters);
                TransactionManager currentTransactionManager = TransactionManager.GetCurrentTransactionManager();
                IDataReader result;
                if (currentTransactionManager == null || !currentTransactionManager.IsSimpleTransaction || !TransactionManager.DalManagedTransactions)
                {
                    result = dbCommand.ExecuteReader(CommandBehavior.CloseConnection);
                }
                else
                {
                    result = dbCommand.ExecuteReader();
                }
                if (processOutput)
                {
                    this.ProcessOutputParams(dbCommand, parameters);
                }
                return result;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Error occured. ProcedureName:{procedureName},Parameters:{parameters}");
                throw;
            }
        }

        public IDataReader GetDataReader(string procedureName, bool processOutput, params object[] parameters)
        {
            IDbConnection connection = DataClassFactory.GetConnection(connectionString);
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return this.GetDataReader(connection, procedureName, processOutput, parameters);
        }

        public DataSet GetDataSet(IDbConnection conIn, string SQL)
        {
            try
            {
                DataAdapter dataAdapter = new DataAdapter(conIn.ConnectionString);
                DataSet dataSet = new DataSet();
                IDbCommand dbCommand = conIn.CreateCommand();
                dbCommand.CommandTimeout = this.m_commandTimeout;
                dbCommand.CommandText = SQL;
                dataAdapter.SelectCommand = dbCommand;
                dataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                dataAdapter.Fill(dataSet);
                return dataSet;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Error occured. SQL:{SQL}");
                throw;
            }
        }

        public DataSet GetDataSet(string connectionString, string SQL, int cacheExpirationIntervalInMinutes)
        {
            DataSet dataSet = this.GetHashedDataSet(connectionString + SQL);
            if (dataSet == null)
            {
                IDbConnection connection = DataClassFactory.GetConnection(connectionString);
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                    dataSet = this.GetDataSet(connection, SQL);
                    connection.Close();
                }
                else
                {
                    dataSet = this.GetDataSet(connection, SQL);
                }
                this.HashDataSet(connectionString + SQL, cacheExpirationIntervalInMinutes, dataSet);
            }
            return dataSet;
        }

        public DataSet GetDataSet(IDbConnection conIn, string procedureName, bool processOutput, params object[] parameters)
        {
            try
            {
                DataAdapter dataAdapter = new DataAdapter(conIn.ConnectionString);
                DataSet dataSet = new DataSet();
                IDbCommand dbCommand = this.BuildCommand(conIn, procedureName, parameters);
                dataAdapter.SelectCommand = dbCommand;
                dataAdapter.Fill(dataSet);
                if (processOutput)
                {
                    this.ProcessOutputParams(dbCommand, parameters);
                }
                return dataSet;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Error occured. ProcedureName:{procedureName},Parameters:{parameters}");
                throw;
            }
        }

        public DataSet GetDataSet(IDbConnection conIn, string procedureName, MissingSchemaAction schemaAction, bool processOutput, params object[] parameters)
        {
            DataAdapter dataAdapter = new DataAdapter(conIn.ConnectionString);
            DataSet dataSet = new DataSet();
            IDbCommand dbCommand = this.BuildCommand(conIn, procedureName, parameters);
            dataAdapter.SelectCommand = dbCommand;
            dataAdapter.MissingSchemaAction = schemaAction;
            dataAdapter.Fill(dataSet);
            if (processOutput)
            {
                this.ProcessOutputParams(dbCommand, parameters);
            }
            return dataSet;
        }

        public DataSet GetDataSet(string connectionString, string procedureName, int cacheExpirationIntervalInMinutes, string dataCacheKey, bool processOutput, params object[] parameters)
        {
            DataSet dataSet = this.GetHashedDataSet(dataCacheKey);
            if (dataSet == null)
            {
                IDbConnection connection = DataClassFactory.GetConnection(connectionString);
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                    dataSet = this.GetDataSet(connection, procedureName, processOutput, parameters);
                    connection.Close();
                }
                else
                {
                    dataSet = this.GetDataSet(connection, procedureName, processOutput, parameters);
                }
                this.HashDataSet(dataCacheKey, cacheExpirationIntervalInMinutes, dataSet);
            }
            return dataSet;
        }

        public DataSet GetDataSet(string connectionString, string procedureName, int cacheExpirationIntervalInMinutes, bool processOutput, params object[] parameters)
        {
            DataSet dataSet = this.GetHashedDataSet(connectionString + procedureName + this.GetConcatenatedParameters(parameters));
            if (dataSet == null)
            {
                IDbConnection connection = DataClassFactory.GetConnection(connectionString);
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                    dataSet = this.GetDataSet(connection, procedureName, processOutput, parameters);
                    connection.Close();
                }
                else
                {
                    dataSet = this.GetDataSet(connection, procedureName, processOutput, parameters);
                }
                this.HashDataSet(connectionString + procedureName, cacheExpirationIntervalInMinutes, dataSet);
            }
            return dataSet;
        }

        private string GetConcatenatedParameters(object[] parameters)
        {
            string text = "";
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    text = text + parameters[i].ToString() + "~";
                }
            }
            return text;
        }

        public DataSet GetDataSet(string procedureName, bool processOutput, params object[] parameters)
        {
            IDbConnection connection = DataClassFactory.GetConnection(connectionString);
            DataSet dataSet;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                dataSet = this.GetDataSet(connection, procedureName, processOutput, parameters);
                connection.Close();
            }
            else
            {
                dataSet = this.GetDataSet(connection, procedureName, processOutput, parameters);
            }
            return dataSet;
        }

        public DataSet GetDataSet(string procedureName, MissingSchemaAction schemaAction, bool processOutput, params object[] parameters)
        {
            IDbConnection connection = DataClassFactory.GetConnection(connectionString);
            DataSet dataSet;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                dataSet = this.GetDataSet(connection, procedureName, schemaAction, processOutput, parameters);
                connection.Close();
            }
            else
            {
                dataSet = this.GetDataSet(connection, procedureName, schemaAction, processOutput, parameters);
            }
            return dataSet;
        }

        private IDbCommand BuildCommand(IDbConnection conIn, string procedureName, params object[] parameters)
        {
            IDbCommand dbCommand = null;
            if (!this.m_debugMode)
            {
                try
                {
                    dbCommand = this.GetHashedCommand(conIn, procedureName);
                    if (dbCommand == null)
                    {
                        dbCommand = conIn.CreateCommand();
                        dbCommand.CommandText = procedureName;
                        dbCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        dbCommand.CommandTimeout = this.m_commandTimeout;
                        DataCommandBuilder.DeriveParameters(dbCommand);
                        this.HashCommand(conIn.ConnectionString, procedureName, dbCommand);
                    }
                    goto IL_7D;
                }
                catch
                {
                    goto IL_7D;
                }
            }
            this.ClearHashedCommands();
            dbCommand = conIn.CreateCommand();
            dbCommand.CommandText = procedureName;
            dbCommand.CommandType = System.Data.CommandType.StoredProcedure;
            dbCommand.CommandTimeout = this.m_commandTimeout;
            DataCommandBuilder.DeriveParameters(dbCommand);
            IL_7D:
            if (parameters != null && dbCommand != null)
            {
                for (int i = 0; i < parameters.GetLength(0); i++)
                {
                    if (parameters[i] == null || (parameters[i] is string && (string)parameters[i] == ""))
                    {
                        ((IDbDataParameter)dbCommand.Parameters[i]).Value = DBNull.Value;
                    }
                    else if (parameters[i] is ThreeValueBool)
                    {
                        if (((ThreeValueBool)parameters[i]).IsNull)
                        {
                            ((IDbDataParameter)dbCommand.Parameters[i]).Value = DBNull.Value;
                        }
                        else
                        {
                            ((IDbDataParameter)dbCommand.Parameters[i]).Value = (bool)((ThreeValueBool)parameters[i]);
                        }
                    }
                    else
                    {
                        ((IDbDataParameter)dbCommand.Parameters[i]).Value = parameters[i];
                    }
                }
            }
            return dbCommand;
        }

        private void ClearHashedCommands()
        {
            cache.Remove("dbcommands");
        }

        private void HashDataSet(string dataCacheKey, int expirationIntervalInMinutes, DataSet cacheData)
        {
            var cacheItem = cache.Get("cacheddatasets");
            if (cacheItem != null)
            {
                this.m_dataCacheItems = (Hashtable)cacheItem.ToObject();
            }
            try
            {
                DataCacheItem value = new DataCacheItem(expirationIntervalInMinutes, cacheData);
                this.m_dataCacheItems.Add(dataCacheKey, value);
                cache.Set("cacheddatasets", this.m_dataCacheItems.ToByteArray());
            }
            catch
            {
            }
        }

        private DataSet GetHashedDataSet(string dataCacheKey)
        {
            DataSet result = null;
            var cacheItem = cache.Get("cacheddatasets");
            if (cacheItem != null)
            {
                this.m_dataCacheItems = (Hashtable)cacheItem.ToObject();
            }
            try
            {
                if (this.m_dataCacheItems.ContainsKey(dataCacheKey))
                {
                    DataCacheItem dataCacheItem = (DataCacheItem)this.m_dataCacheItems[dataCacheKey];
                    if (!dataCacheItem.HasExpired)
                    {
                        result = dataCacheItem.CachedData;
                    }
                    else
                    {
                        this.m_dataCacheItems.Remove(dataCacheKey);
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        private IDbCommand GetHashedCommand(IDbConnection conIn, string procedureName)
        {
            var cacheItem = cache.Get("dbcommands");
            if (cacheItem != null)
            {
                this.m_commandHash = (Hashtable)cacheItem.ToObject();
            }
            try
            {
                if (this.m_commandHash.ContainsKey(conIn.ConnectionString + procedureName))
                {
                    IDbCommand dbCommand = conIn.CreateCommand();
                    dbCommand.CommandText = procedureName;
                    dbCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    dbCommand.CommandTimeout = this.m_commandTimeout;
                    IDbDataParameter[] array = (IDbDataParameter[])this.m_commandHash[conIn.ConnectionString + procedureName];
                    for (int i = 0; i < array.Length; i++)
                    {
                        dbCommand.Parameters.Add(this.CopyParameter(array[i], conIn.ConnectionString));
                    }
                    return dbCommand;
                }
            }
            catch
            {
            }
            return null;
        }

        private void HashCommand(string connectionString, string procedureName, IDbCommand cmdIn)
        {
            IDbDataParameter[] array = new IDbDataParameter[cmdIn.Parameters.Count];
            for (int i = 0; i < cmdIn.Parameters.Count; i++)
            {
                array[i] = this.CopyParameter((IDbDataParameter)cmdIn.Parameters[i], connectionString);
            }
            this.m_commandHash.Add(connectionString + procedureName, array);
            cache.Set("dbcommands", this.m_dataCacheItems.ToByteArray());
        }

        private IDbDataParameter CopyParameter(IDbDataParameter prmIn, string connectionString)
        {
            IDbDataParameter parameter = DataClassFactory.GetParameter(connectionString);
            parameter.DbType = prmIn.DbType;
            parameter.Direction = prmIn.Direction;
            parameter.ParameterName = prmIn.ParameterName;
            parameter.Precision = prmIn.Precision;
            parameter.Scale = prmIn.Scale;
            parameter.Size = prmIn.Size;
            parameter.SourceColumn = prmIn.SourceColumn;
            parameter.SourceVersion = prmIn.SourceVersion;
            return parameter;
        }

        private void ProcessOutputParams(IDbCommand cmdTemp, object[] parameters)
        {
            for (int i = 0; i < cmdTemp.Parameters.Count; i++)
            {
                IDbDataParameter dbDataParameter = (IDbDataParameter)cmdTemp.Parameters[i];
                if (dbDataParameter.Direction != ParameterDirection.Input)
                {
                    parameters[i] = dbDataParameter.Value;
                }
            }
        }

        public void ClearDataCache()
        {
            cache.Remove("cacheddatasets"); 
        }

        public const int DEFAULT_COMMAND_TIMEOUT = 30;
        private readonly IDistributedCache cache;
        private readonly ILogger<DataAccessLayer> logger;
        private bool m_debugMode;
        private Hashtable m_commandHash = new Hashtable();
        private Hashtable m_dataCacheItems = new Hashtable();
        private int m_commandTimeout;
    }
}
