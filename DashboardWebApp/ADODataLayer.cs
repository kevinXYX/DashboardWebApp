using System.Data;
using System.Data.SqlClient;

namespace DashboardWebApp
{
    public class ADODataLayer
    {
        private readonly string connectionString;

        public ADODataLayer(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public DataSet GetDataSet(string procedureName, SqlParameter[] parameters)
        {
            DataSet ds = new DataSet();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(procedureName, conn))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddRange(parameters);
                    adapter.Fill(ds);
                }
            }

            return ds;
        }

        public int ExecuteSP(string procedureName, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                try
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(procedureName, conn))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                        adapter.SelectCommand.Parameters.AddRange(parameters);
                        return adapter.SelectCommand.ExecuteNonQuery();
                    }
                }
                finally 
                {
                    conn.Close();
                }
            }
        }
    }
}
