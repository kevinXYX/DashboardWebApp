using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace DashboardWebApp.Data
{
	public class DataAdapter
	{
		public DataAdapter(string connectionString)
		{
			this.m_DBType = DataClassFactory.GetDBType(connectionString);
			this.m_adapterInterface = new SqlDataAdapter();
		}

		public IDbCommand SelectCommand
		{
			set
			{
				((SqlDataAdapter)this.m_adapterInterface).SelectCommand = (SqlCommand)value;
			}
		}

		public MissingSchemaAction MissingSchemaAction
		{
			get
			{
				return this.m_adapterInterface.MissingSchemaAction;
			}
			set
			{
				this.m_adapterInterface.MissingSchemaAction = value;
			}
		}

		public int Fill(DataSet dstIn)
		{
			return this.m_adapterInterface.Fill(dstIn);
		}

		private DBType m_DBType;
		private IDataAdapter m_adapterInterface;
	}
}
