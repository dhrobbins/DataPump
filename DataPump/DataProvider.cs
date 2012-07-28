using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Data;
using System.Data.SqlClient;
using DataTablePager.Utils;

namespace DataPump
{
    public class DataProvider
    {
        public string ConnectionString { get; set; }
        public string Procedure { get; set; }
                
        public List<SqlParameter> Parameters
        {
            get { return GetSqlParametersAsList() ; }
        }
        
        private SqlCommand sqlCmd;
        private Dictionary<string, object> parameterObjects;

        public DataProvider(string connection, string procedure) 
        {
            Enforce.That(string.IsNullOrEmpty(connection) == false,
                            "DataProvider() - connection can not be null");

            Enforce.That(string.IsNullOrEmpty(procedure) == false,
                            "DataProvider() - connection can not be null");
            
            //  ToDo:  Detect if connection name is valid
            this.ConnectionString = connection;
            this.sqlCmd = new SqlCommand();
            this.Procedure = procedure;
            this.parameterObjects = new Dictionary<string, object>();

            InitializeCommand();
        }

        public DataProvider SetParameters(params object[] args)
        {
            foreach (var item in args)
            {
                AddParameter(item, string.Format("@{0}", sqlCmd.Parameters.Count));
            }

            return this;
        }

        public DataProvider SetParameters(Dictionary<string, object> parameters)
        {
            parameters.ToList()
                       .ForEach(x => AddParameter(x.Value, x.Key));
            
            return this;
        }

        /// <summary>
        /// Stolen from Massive by Rob Connery
        /// </summary>
        /// <param name="item"></param>
        private void AddParameter(object item, string parameterName)
        {
            var parameter = this.sqlCmd.CreateParameter();
            parameter.ParameterName = parameterName;
            if (item == null)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                if (item.GetType() == typeof(Guid))
                {
                    parameter.Value = item.ToString();
                    parameter.DbType = DbType.String;
                    parameter.Size = 4000;
                }
                else if (item.GetType() == typeof(ExpandoObject))
                {
                    var d = (IDictionary<string, object>)item;
                    parameter.Value = d.Values.FirstOrDefault();
                }
                else
                {
                    parameter.Value = item;
                }
                if (item.GetType() == typeof(string))
                    parameter.Size = ((string)item).Length > 4000 ? -1 : 4000;
            }
            sqlCmd.Parameters.Add(parameter);
        }

        private List<SqlParameter> GetSqlParametersAsList()
        {
            var parameters = new List<SqlParameter>();
            
            foreach (var item in this.sqlCmd.Parameters)
            {
                if (((SqlParameter)item).Direction ==ParameterDirection.Input)
                {
                    parameters.Add((SqlParameter)item);
                }
            }

            return parameters;
        }

        public List<dynamic> ExecuteStoredProcedure()
        {
            Enforce.That(string.IsNullOrEmpty(this.Procedure) == false, 
                            "DataProvider.GetProcedureData - procedure can not be null");
            Enforce.That(string.IsNullOrEmpty(this.ConnectionString) == false,
                            "DataProvider.GetProcedureData - ConnectionString can not be null");

            var dataRows = new List<dynamic>();
            var dataReader = sqlCmd.ExecuteReader(CommandBehavior.CloseConnection);

            while(dataReader.Read())
            {
                dynamic recordSet = new ExpandoObject();
                var record = recordSet as IDictionary<string, object>;

                int i = 0;
                int columnCount = dataReader.FieldCount;

                for (i = 0; i < columnCount; i++ )
                {
                    record.Add( dataReader.GetName(i), 
                                    DBNull.Value.Equals(dataReader[i]) ? null : dataReader[i]);                    
                }

                dataRows.Add(record);
            }

            dataReader.Close();
            return dataRows;
        }

        public DataProvider InitializeCommand()
        {
            var sqlConnection = new SqlConnection(this.ConnectionString);
            sqlConnection.Open();

            sqlCmd = new SqlCommand(this.Procedure, sqlConnection);
            sqlCmd.CommandType = CommandType.StoredProcedure;

            return this;
        }

        public DataProvider ReadStoreProcedureSchema(string procedure)
        {
            Enforce.That(string.IsNullOrEmpty(procedure) == false,
                            "DataProvider.ReadStoredProcedureSchema - procedure can not be null");
            
            var sqlConnection = new SqlConnection(this.ConnectionString);
            sqlConnection.Open();

            sqlCmd = new SqlCommand(procedure, sqlConnection);
            sqlCmd.CommandType = CommandType.StoredProcedure;
            SqlCommandBuilder.DeriveParameters(sqlCmd);
            sqlConnection.Close();

            return this;
        }
    }
}
