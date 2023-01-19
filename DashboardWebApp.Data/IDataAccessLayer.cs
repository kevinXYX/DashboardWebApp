using System.Data;

namespace DashboardWebApp.Data
{
    public interface IDataAccessLayer
    {
        int CommandTimeout { get; set; }
        void ClearDataCache();
        int ExecuteCommand(string procedureName, bool processOutput, params object[] parameters);
        IDataReader GetDataReader(IDbConnection conIn, string SQL);
        IDataReader GetDataReader(IDbConnection conIn, string procedureName, bool processOutput, params object[] parameters);
        IDataReader GetDataReader(string connectionString, string SQL);
        IDataReader GetDataReader(string procedureName, bool processOutput, params object[] parameters);
        DataSet GetDataSet(IDbConnection conIn, string SQL);
        DataSet GetDataSet(IDbConnection conIn, string procedureName, bool processOutput, params object[] parameters);
        DataSet GetDataSet(IDbConnection conIn, string procedureName, MissingSchemaAction schemaAction, bool processOutput, params object[] parameters);
        DataSet GetDataSet(string procedureName, bool processOutput, params object[] parameters);
        DataSet GetDataSet(string connectionString, string SQL, int cacheExpirationIntervalInMinutes);
        DataSet GetDataSet(string connectionString, string procedureName, int cacheExpirationIntervalInMinutes, bool processOutput, params object[] parameters);
        DataSet GetDataSet(string connectionString, string procedureName, int cacheExpirationIntervalInMinutes, string dataCacheKey, bool processOutput, params object[] parameters);
    }
}