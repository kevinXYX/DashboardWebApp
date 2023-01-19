using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Transactions;

namespace DashboardWebApp.Data
{
	public class DataClassFactory
	{
		internal static IDbConnection GetConnection(string connectionString)
		{
			IDbConnection dbConnection = null;
			bool flag = false;
			if (Transaction.Current != null && TransactionManager.DalManagedTransactions)
			{
				TransactionManager transactionManager = TransactionManager.GetCurrentTransactionManager();
				if (transactionManager != null)
				{
					if (Transaction.Current.TransactionInformation.LocalIdentifier != transactionManager.GetLastAddedTransactionLocalIdentifier())
					{
						transactionManager.AddTransaction(Transaction.Current);
						flag = true;
					}
					else if (transactionManager.IsSimpleTransaction)
					{
						dbConnection = transactionManager.GetConnection(connectionString, Transaction.Current.TransactionInformation.LocalIdentifier);
						if (dbConnection == null)
						{
							transactionManager.IsSimpleTransaction = false;
						}
					}
				}
				else
				{
					transactionManager = new TransactionManager(Transaction.Current);
					TransactionManager.SetTransactionManager(transactionManager);
					flag = true;
				}
			}
			if (dbConnection == null)
			{
				dbConnection = new SqlConnection(connectionString);

				if (flag)
				{
					dbConnection.Open();
					TransactionManager currentTransactionManager = TransactionManager.GetCurrentTransactionManager();
					currentTransactionManager?.AddConnection(Transaction.Current, connectionString, dbConnection);
				}
			}
			return dbConnection;
		}

		public static IDbCommand GetCommand(string connectionString)
		{
			return new SqlCommand();
		}

		public static IDbDataParameter GetParameter(string connectionString)
		{
			return new SqlParameter();
		}

		public static DBType GetDBType(string connectionString)
		{
			return DBType.DB_TYPE_SQL;
		}

		public static CommandType GetCommandType(string connectionString)
		{
			return CommandType.NORMAL;
		}
	}
}
