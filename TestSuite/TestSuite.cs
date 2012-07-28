using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.ComponentModel;
using DataPump;
using NUnit.Framework;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Xml;

namespace TestSuite
{
    [TestFixture]
    public class TestSuite
    {
        private string connection = "Data Source=sql_mri;Initial Catalog=TenantMaster;User ID=sa;Password=protectsa";
        

        //"usp_GetLLXDealsCalYearForSharepoint"
        //"usp_GetLLXDealsWITYYearForSharepoint"
        //"usp_GetLLXSumsCalYearForSharepoint"
        //"usp_GetLLXSumsWITYYearForSharepoint"
        //"usp_pivotLLXWityYear"

        [Test]
        [NUnit.Framework.Category("DataProvider")]
        public void CanExecuteStoredProcedure()
        {
            string procedure = "usp_GetLLXDealsCalYearForSharepoint";
            var dataProvider = new DataProvider(connection, procedure);
            var results = dataProvider.ExecuteStoredProcedure();

            int actualCount = StoredProcedureCount(procedure);

            Assert.AreEqual(results.Count, actualCount);

        }

        [Test]
        [NUnit.Framework.Category("DataProvider")]
        public void CanSerializeExpandoListToJson()
        {
            string procedure = "usp_GetLLXDealsCalYearForSharepoint";
            var dataProvider = new DataProvider(connection, procedure);
            var results = dataProvider.ExecuteStoredProcedure();

            string json = JsonConvert.SerializeObject(results);
            Assert.IsTrue(json.Length > 0);
        }

        [Test]
        public void CanAddSQLParametersToProvider()
        {
            string procedure = "usp_GetLLXDealsCalYearForSharepoint";
            var dataProvider = new DataProvider(connection, procedure);

            object integerParm = 5;
            object decimalParm = 5.6;
            object stringParm = "a line of text";
            object datetimeParm = DateTime.Now;

            var parameters = new Dictionary<string, object>();
            parameters.Add("integerParm", integerParm);
            parameters.Add("decimalParm", decimalParm);
            parameters.Add("stringParm", stringParm);
            parameters.Add("datatimeParm", datetimeParm);

            dataProvider.SetParameters(integerParm, decimalParm, stringParm, datetimeParm);

            Assert.AreEqual(4, dataProvider.Parameters.Count);
        }

        [Test]
        [NUnit.Framework.Category("DataProvider")]
        public void CanAddNamedSQLParametersToProvider()
        {
            string procedure = "usp_GetLLXDealsCalYearForSharepoint";
            var dataProvider = new DataProvider(connection, procedure);
            
            object integerParm = 5;
            object decimalParm = 5.6;
            object stringParm = "a line of text";
            object datetimeParm = DateTime.Now;

            var parameters = new Dictionary<string, object>();
            parameters.Add("integerParm", integerParm);
            parameters.Add("decimalParm", decimalParm);
            parameters.Add("stringParm", stringParm);
            parameters.Add("datatimeParm", datetimeParm);

            dataProvider.SetParameters(parameters);

            Assert.AreEqual(4, dataProvider.Parameters.Count);
            var resultStringParm = dataProvider.Parameters
                                                .Where(x => x.ParameterName == "stringParm")
                                                .ToList();
                                                
            Assert.AreEqual("stringParm", resultStringParm[0].ParameterName);
        }

        [Test]
        [NUnit.Framework.Category("DataProvider")]
        public void CanReadStoredProcedureSchema()
        {
            string procedure = "sp_Report_Agent_Wity_Listing";
            var dataProvider = new DataProvider(connection, procedure);

            var sqlParms = dataProvider.ReadStoreProcedureSchema(procedure)
                                        .Parameters;

            Assert.AreEqual(4, sqlParms.Count);
        }

        [Test]
        [NUnit.Framework.Category("DataProvider")]
        public void CanExecuteStoredProcWithParms()
        {
            //  15,'07/01/09','06/30/10','pai,app'
            string procedure = "sp_Report_Agent_Wity_Listing";
            var dataProvider = new DataProvider(connection, procedure);

            var parameters = new Dictionary<string, object>();
            parameters.Add("AgentId", 15);
            parameters.Add("StartDate", new DateTime(2009,7, 1));
            parameters.Add("EndDate", new DateTime(2010, 6, 30));
            parameters.Add("StatusList", "pai,app");

            var results = dataProvider.SetParameters(parameters)
                            .ExecuteStoredProcedure();

            Assert.AreEqual(36, results.Count);
        }

        [Test]
        [NUnit.Framework.Category("DataProvider")]
        public void CanIterateListOfDynamic()
        {
            //  15,'07/01/09','06/30/10','pai,app'
            string procedure = "sp_Report_Agent_Wity_Listing";
            var dataProvider = new DataProvider(connection, procedure);

            var parameters = new Dictionary<string, object>();
            parameters.Add("AgentId", 15);
            parameters.Add("StartDate", new DateTime(2009, 7, 1));
            parameters.Add("EndDate", new DateTime(2010, 6, 30));
            parameters.Add("StatusList", "pai,app");

            //var results = dataProvider.SetParameters(parameters)
            //                .ExecuteStoredProcedure();

            List<dynamic> results = dataProvider.SetParameters(parameters)
                            .ExecuteStoredProcedure();

            Assert.AreEqual(36, results.Count);

            IDictionary<string, object> record = (IDictionary<string, object>)results[0];
            Assert.AreEqual(15, record["AgentID"]);
        }

        [Test]
        [NUnit.Framework.Category("DataProvider")]
        public void TestExpando()
        {
            dynamic root = new ExpandoObject();
            root.Name = "Name";

            var result = GetExpandos();

            root.Child = result;

            var first = Enumerable.First(root.Child);

            Assert.AreEqual("Obj1", first.Name);
        }

        [Test]
        [NUnit.Framework.Category("TypeConversion")]
        public void CanConvertStringToInt()
        {
            var queryStringConverter = new QueryStringConverter();
            
            var nvCollection = new NameValueCollection();
            nvCollection.Add("TheInteger", "32");

            var results = queryStringConverter.ConvertToParameters(nvCollection);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(32, results["TheInteger"]);
        }

        [Test]
        [NUnit.Framework.Category("TypeConversion")]
        public void CanConvertStringToDateTime()
        {
            var queryStringConverter = new QueryStringConverter();

            var nvCollection = new NameValueCollection();
            var theDate = new DateTime(2012, 12, 31);
            nvCollection.Add("TheDate", theDate.ToShortDateString());
            
            var results = queryStringConverter.ConvertToParameters(nvCollection);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(theDate, results["TheDate"]);
        }

        [Test]
        [NUnit.Framework.Category("TypeConversion")]
        public void CanConvertStringOfDatesToString()
        {
            string theDateString = @"12/31/2012,1/1/2011";
            var nvCollection = new NameValueCollection();
            nvCollection.Add("DateString", theDateString);

            var queryStringConverter = new QueryStringConverter();
            var results = queryStringConverter.ConvertToParameters(nvCollection);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(theDateString, results["DateString"].ToString());

        }

        [Test]
        [NUnit.Framework.Category("TypeConversion")]
        public void CanExcludeProcedureAndConnectionFromConversion()
        {
            string theDateString = @"12/31/2012,1/1/2011";
            var nvCollection = new NameValueCollection();
            nvCollection.Add("DateString", theDateString);
            nvCollection.Add("procedure", "sp_procedure");
            nvCollection.Add("Connection", "the connection");


            var queryStringConverter = new QueryStringConverter();
            var results = queryStringConverter.ConvertToParameters(nvCollection);
            
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(theDateString, results["DateString"].ToString());
            Assert.AreEqual("sp_procedure", queryStringConverter.StoredProcedure);
            Assert.AreEqual("the connection", queryStringConverter.ConnectionName);

        }

        [Test]
        [NUnit.Framework.Category("TypeConversion")]
        public void CanExecProceWithConvertedParms()
        {
            //  15,'07/01/09','06/30/10','pai,app'
            int agentId = 15;
            DateTime startDate = new DateTime(2009, 7, 1);
            DateTime endDate = new DateTime(2010, 6, 30);
            string statusList = "pai,app";

            string procedure = "sp_Report_Agent_Wity_Listing";
            var dataProvider = new DataProvider(connection, procedure);

            var nvCollection = new NameValueCollection();
            nvCollection.Add("AgentId", agentId.ToString());
            nvCollection.Add("StartDate", startDate.ToShortDateString());
            nvCollection.Add("EndDate", endDate.ToShortDateString());
            nvCollection.Add("StatusList", statusList);

            var queryStringConverter = new QueryStringConverter();
            var parameters = queryStringConverter.ConvertToParameters(nvCollection);
            var results = dataProvider.SetParameters(parameters)
                                        .ExecuteStoredProcedure();

            Assert.AreEqual(36, results.Count);
            
        }

        [Test]
        [NUnit.Framework.Category("TypeConversion")]
        public void CanConvertDelimitedStringToParameters()
        {
            //  15,'07/01/09','06/30/10','pai,app'
            string parameterSource = @"AgentID=15;StartDate=07/01/09;EndDate=06/30/10;StatusList=pai,app";

            var queryStringConverter = new QueryStringConverter();
            var converted = queryStringConverter.ConvertToParameters(parameterSource);

            Assert.AreEqual(15, converted["AgentID"]);
        }

        [Test]
        [NUnit.Framework.Category("Serialize")]
        public void CanSerializeListOfDynamicToXML()
        { 
            //usp_GetLLXDealsCalYearForSharepoint

            var dataProvider = new DataProvider(connection, "usp_GetLLXCalYearForSharepoint");
            var results = dataProvider.ExecuteStoredProcedure();

            string json = JsonConvert.SerializeObject(results);

            string rootStart = @"{'root' : {'records' : ";
            string end = @"}}";
            json = rootStart + json + end;

            System.Xml.XmlNode myXmlNode = (XmlNode)JsonConvert.DeserializeXmlNode(json);
            Console.WriteLine(myXmlNode.InnerXml);

            Assert.IsTrue(myXmlNode.InnerXml.Length > 1);
        }

        private int StoredProcedureCount(string procedure)
        {
            var sqlConnection = new SqlConnection(connection);
            sqlConnection.Open();

            var sqlCmd = new SqlCommand(procedure, sqlConnection);
            sqlCmd.CommandType = CommandType.StoredProcedure;

            var dataReader = sqlCmd.ExecuteReader(CommandBehavior.CloseConnection);

            int count = 0;
            while (dataReader.Read())
            { 
                count++;
            }

            dataReader.Close();

            return count;
        }

        private IEnumerable<dynamic> GetExpandos()
        {
            var toReturn = new List<dynamic>();

            dynamic obj1 = new ExpandoObject();
            toReturn.Add(obj1);
            obj1.Name = "Obj1";

            dynamic obj2 = new ExpandoObject();
            toReturn.Add(obj2);
            obj2.Name = "Obj2";

            return toReturn;
        }
    }
}
