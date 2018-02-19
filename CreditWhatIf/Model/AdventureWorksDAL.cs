using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditWhatIf.Model
{
    public static class AdventureWorksDAL
    {
        public static DataTable GetData()
        {
            string sConn = @"Server=(local); Database=AdventureWorks;Trusted_Connection=true;User Id=ABSAROKA\frann";
            SqlConnection conn = new SqlConnection(sConn);
            string sSQL = "SELECT * FROM [Production].[Product] ORDER BY ProductLine, Name";
            SqlCommand comm;
            SqlDataReader DR;
            DataTable DT = new DataTable();
            DataTable UoM = new DataTable();

            conn.Open();
            comm = new SqlCommand(sSQL, conn);
            DR = comm.ExecuteReader();
            DT.Load(DR);            
            DR.Close();
            conn.Close();
            return DT;
        }
    }
}
