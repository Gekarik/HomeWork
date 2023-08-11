using System.Data.SqlClient;
using System.Data;


namespace Home_Work.Scripts
{
    internal class DBService:IDisposable
    {
        public void Dispose() 
        {
            if(sqlConnection.State == ConnectionState.Open)
                CloseConnection();            
        }

        readonly SqlConnection sqlConnection = new (ConfigLoader.GetConfiguration()["ConnectionString"]);
        readonly string insertValuesToOpenAi = ConfigLoader.GetConfiguration()["SQLInsetToOpenAi"];
        readonly string insertToVakSms = ConfigLoader.GetConfiguration()["SQLInsertToVakSms"];
        
        public int OpenAiId { get; set; }
        public void OpenConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                sqlConnection.Open();
            }
        }
        public void CloseConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Open)
            {
                sqlConnection.Close();
            }
        }
        public SqlConnection GetConnection()
        {
            return sqlConnection;
        }
        public void AddInformation(OpenAi openAi, VakSms phone)
        {            
            OpenConnection();
            SqlTransaction transaction = sqlConnection.BeginTransaction();
            try
            {
                var cmd1 = new SqlCommand(insertValuesToOpenAi, sqlConnection)
                {
                    Transaction = transaction
                };
                AddOpenAiInfo(openAi,cmd1);

                var cmd2 = new SqlCommand(insertToVakSms, sqlConnection)
                {
                    Transaction = transaction
                };

                AddVakSmsInfo(phone, cmd2);
                                
                transaction.Commit();
            }
            catch (Exception)
            {                
                transaction.Rollback();
                throw;
            }
            finally
            {                
                CloseConnection();
            }
        }
        public void AddOpenAiInfo(OpenAi openAi,SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("FirstName", openAi.FirstName);
            cmd.Parameters.AddWithValue("LastName", openAi.LastName);
            cmd.Parameters.AddWithValue("email", openAi.Email);
            cmd.Parameters.AddWithValue("pass", openAi.Password);
            cmd.Parameters.AddWithValue("apikey", openAi.Apikey);
            cmd.Parameters.AddWithValue("birthday", openAi.Birthday);
            cmd.Parameters.AddWithValue("regtime", openAi.RegTime);
            cmd.Parameters.AddWithValue("deathTime", openAi.DeathTime);
            OpenAiId = Convert.ToInt32(cmd.ExecuteScalar());
        }
        public void AddVakSmsInfo(VakSms phone, SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("apikey", phone.ApiKey);
            cmd.Parameters.AddWithValue("tel", phone.Tel);
            cmd.Parameters.AddWithValue("smsCode", phone.SmsCode);
            cmd.Parameters.AddWithValue("idNum", phone.IdNum);
            cmd.Parameters.AddWithValue("_service", phone.Service);
            cmd.Parameters.AddWithValue("oper", phone.Operator);
            cmd.Parameters.AddWithValue("country", phone.Country);
            cmd.Parameters.AddWithValue("openAi_id", OpenAiId);
            cmd.ExecuteNonQuery();
        }
    }
}
