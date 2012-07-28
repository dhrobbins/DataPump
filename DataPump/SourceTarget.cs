using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using DataPump;
using System.Configuration;

namespace DataPump
{
    [Serializable]
    public class SourceTarget
    {
        public int SourceTargetId { get; set; }
        public string SQLSource { get; set; }
        public string SharePointTarget { get; set; }
        public string ConnectionString { get; set; }
        public string ListId { get; set; }
        public string ConnectionName { get; set; }
        
        //  Being Lazy - place holder for grid display.  Don't want to do dynamic
        public string ImageRef { get; set; }

        public Dictionary<string, object> ConvertToParameters()
        {
            var parameters = new Dictionary<string, object>();

            parameters.Add("SourceTargetId", this.SourceTargetId);
            parameters.Add("SQLSource", this.SQLSource);
            parameters.Add("SharePointTarget", this.SharePointTarget);
            parameters.Add("ConnectionString", this.ConnectionString);
            parameters.Add("ListId", this.ListId);
            parameters.Add("ConnectionName", this.ConnectionName);
            
            return parameters;
        }
    }

    public class SourceTargetManager
    {
        private string connection;
        private string procedure;
        
        public SourceTargetManager(string connection, string procedure)
        {
            this.connection = connection;
            this.procedure = procedure;
        }

        public List<SourceTarget> GetSourceTargets()
        {
            var sourceTargets = new List<SourceTarget>();
            var sqlConnection = new SqlConnection(this.connection);
            sqlConnection.Open();

            var sqlCmd = new SqlCommand(this.procedure, sqlConnection);
            sqlCmd.CommandType = CommandType.StoredProcedure;

            var dataReader = sqlCmd.ExecuteReader(CommandBehavior.CloseConnection);

            while (dataReader.Read())
            {
                var sourceTarget = new SourceTarget();

                sourceTarget.SourceTargetId = Convert.ToInt32(dataReader["SourceTargetId"]);
                sourceTarget.ConnectionName = dataReader["ConnectionName"].ToString();
                sourceTarget.ConnectionString = dataReader["ConnectionString"].ToString();
                sourceTarget.ListId = dataReader["ListId"].ToString();
                sourceTarget.SharePointTarget = dataReader["SharePointTarget"].ToString();
                sourceTarget.SQLSource = dataReader["SQLSource"].ToString();
                sourceTarget.ImageRef = string.Empty;

                sourceTargets.Add(sourceTarget);
            }

            dataReader.Close();
            
            return sourceTargets;
        }

        public void InsertSourceTarget(SourceTarget sourceTarget)
        {
            string procedure = ConfigurationManager.AppSettings["InsertSourceTarget"].ToString();
            ExecuteProcedure(procedure, sourceTarget);
        }

        public void UpdateSourceTarget(SourceTarget sourceTarget)
        {
            string procedure = ConfigurationManager.AppSettings["UpdateSourceTarget"].ToString();
            ExecuteProcedure(procedure, sourceTarget);
        }

        public void DeleteSourceTarget(int sourceTargetId)
        {
            var sourceTarget = new SourceTarget();
            sourceTarget.SourceTargetId = sourceTargetId;
            
            string procedure = ConfigurationManager.AppSettings["DeleteSourceTarget"].ToString();
            ExecuteProcedure(procedure, sourceTarget);
        }

        private void ExecuteProcedure(string procedure, SourceTarget sourceTarget)
        {
            var dataProvider = new DataProvider(this.connection, procedure);
            dataProvider.SetParameters(sourceTarget.ConvertToParameters())
                            .ExecuteStoredProcedure();
        }
    }
}
