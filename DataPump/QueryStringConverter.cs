using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using DataTablePager.Utils;

namespace DataPump
{
    public class QueryStringConverter
    {

        public string ConnectionName { get; set; }
        public string StoredProcedure { get; set; }

        public QueryStringConverter() { }

        public Dictionary<string, object> ConvertToParameters(NameValueCollection httpRequestNVPairs)
        {
            var requests = CloneNameValueCollection(httpRequestNVPairs);
            
            //  Is stored procedure included?
            string procedure = requests["Procedure"];
            string connection = requests["Connection"];

            if (string.IsNullOrEmpty(procedure))
            {
                throw new ApplicationException("QueryStringConverter.ConvertToParameters - Procedure not included");
            }
            else
            {
                this.StoredProcedure = procedure;
                requests.Remove("Procedure");
            }

            //  Is connection name included?
            if(string.IsNullOrEmpty(connection))
            {
                throw new ApplicationException("QueryStringConverter.ConvertToParameters - Connection not included");
            }
            else
            {
                this.ConnectionName = connection;
                requests.Remove("Connection");
            }


            var parameters = new Dictionary<string, object>();
            
            String[] keys = requests.AllKeys;
            int keyLength = keys.Length;
            int i;
            int j;

            for (i = 0; i < keyLength; i++ )
            {
                string theKey = keys[i];
                String[] values = requests.GetValues(theKey);

                for (j = 0; j < values.Length; j++)
                { 
                    parameters.Add(theKey, ConvertToObject(values[j]));                                           
                }
            }

            return parameters;
        }

        public Dictionary<string, object> ConvertToParameters(string parameterSource)
        {
            Enforce.That(string.IsNullOrEmpty(parameterSource) == false, 
                                "QueryStringConverter.ConvertToParameters - parameterSource can not be null");
            
            var parameters = new Dictionary<string, object>();

            var parameterStrings = parameterSource.Split(';');
            foreach (string parmString in parameterStrings)
            {
                string[] nameValue = parmString.Split('=');
                parameters.Add(nameValue[0], ConvertToObject(nameValue[1]));
            }
            
            return parameters;
        }

        private NameValueCollection CloneNameValueCollection(NameValueCollection source)
        {
            var newCollection = new NameValueCollection();

            foreach (string key in source)
            {
                foreach (string value in source.GetValues(key))
                {
                    newCollection.Add(key, value);
                }
            }

            return newCollection;
        }

        private object ConvertToObject(string value)
        {
            if (IsInteger(value))
            {
                return Decimal.Parse(value);
            }

            if (IsDecimal(value))
            {
                return Int32.Parse(value);
            }

            if (IsDateTime(value))
            {
                return DateTime.Parse(value);
            }

            if (IsBoolean(value))
            {
                return Boolean.Parse(value);
            }

            return value;
        }

        private bool IsInteger(string value)
        {
            Int32 dummy;
            return Int32.TryParse(value, out dummy);
        }

        private bool IsDecimal(string value)
        {
            decimal dummy;
            return Decimal.TryParse(value, out dummy);
        }

        private bool IsDateTime(string value)
        {
            DateTime dummy;
            return DateTime.TryParse(value, out dummy);
        }

        private bool IsBoolean(string value)
        {
            if((value.Trim().ToLower() == "false") ||(value.Trim().ToLower() == "true"))
            {
                return true;
            }

            return false;
        }
    }
}
