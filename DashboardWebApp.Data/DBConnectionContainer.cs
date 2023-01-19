using System;
using System.Data;

namespace DashboardWebApp.Data
{
	internal class DBConnectionContainer
	{
		internal string Key
		{
			get
			{
				return this.m_key;
			}
			set
			{
				this.m_key = value;
			}
		}

		internal IDbConnection Value
		{
			get
			{
				return this.m_value;
			}
			set
			{
				this.m_value = value;
			}
		}

		public string TransactionId
		{
			get
			{
				return this.m_transactionId;
			}
			set
			{
				this.m_transactionId = value;
			}
		}

		internal DBConnectionContainer(string transactionId)
		{
			this.m_transactionId = transactionId;
		}

		internal DBConnectionContainer(string key, string connectionString, IDbConnection conn)
		{
			this.m_key = key;
			this.m_value = conn;
		}

		private string m_key = "";
		private IDbConnection m_value;
		private string m_transactionId = "";
	}
}
