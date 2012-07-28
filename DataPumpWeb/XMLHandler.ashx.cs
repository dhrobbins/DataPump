using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataPump;
using System.Configuration;
using System.Text;
using Newtonsoft.Json;
using System.Xml;

namespace DataPumpWeb
{
    /// <summary>
    /// Summary description for XMLHandler
    /// </summary>
    public class XMLHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var queryStringConverter = new QueryStringConverter();
            var parameters = queryStringConverter
                                .ConvertToParameters(context.Request.QueryString);

            string procedure = queryStringConverter.StoredProcedure;
            string connectionName = queryStringConverter.ConnectionName;

            context.Response.ContentType = "text/xml";
            context.Response.ContentEncoding = Encoding.UTF8;

            List<SourceTarget> sourceTargets = context.Application["SourceTargets"] as List<SourceTarget>;

            //  Does this procedure and connection combo exist
            var matchedSourceTarget = FetchMatchingSourceTarget(procedure,
                                    connectionName,
                                    sourceTargets);

            if (matchedSourceTarget != null)
            {
                var dataProvider = new DataProvider(matchedSourceTarget.ConnectionString, procedure);
                var records = dataProvider.SetParameters(parameters)
                                            .ExecuteStoredProcedure();
                string jsonRecords = "{'Root' : {'Record' : " + 
                                        JsonConvert.SerializeObject(records)
                                        + "}}";
                System.Xml.XmlNode myXmlNode = (XmlNode)JsonConvert.DeserializeXmlNode(jsonRecords);

                context.Response.Write(myXmlNode.InnerXml);
            }
            else
            {
                context.Response.Write(string.Empty);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private SourceTarget FetchMatchingSourceTarget(string procedure, string connectionName,
                                            List<SourceTarget> sourceTargets)
        {
            return sourceTargets.Find(x => (x.ConnectionName.ToLower() == connectionName.ToLower()) &&
                                                    x.SQLSource.ToLower() == procedure.ToLower());
        }
    }
}