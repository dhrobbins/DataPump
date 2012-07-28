DataPump - A Data Access Layer for Web Services
===============================================

Introduction
------------
DataPump is a database access layer tool that uses stored procedures for pulling and pushing data from SQL Server.  DataPump executes a stored procedure and creates a result set of List<dynamic>.  Additionally, DataPump provides a webservice that allows you to pass the name of stored procedure and parameters, then returns the result as either Json or Xml.  

With DataPump you can quickly create a centralized web service and add new data sources by merely issuing calls to stored procedures.  There is also a white list mechanism to control what stored procedures may be used.


### Executing code to get data
A typical set of statements that use DataPump are as follows:

```csharp
string connection = "Data Source=sql_db_server;Initial Catalog=Tenants;User ID=dbuser;Password=dbuserpwd";
string procedure = "sp_Report_Sales_Agent_Listing";
var dataProvider = new DataProvider(connection, procedure);

var parameters = new Dictionary<string, object>();
parameters.Add("AgentId", 15);
parameters.Add("StartDate", new DateTime(2009, 7, 1));
parameters.Add("EndDate", new DateTime(2010, 6, 30));
parameters.Add("StatusList", "pai,app");

List<dynamic> results = dataProvider.SetParameters(parameters)
				.ExecuteStoredProcedure();
```
				
				
### Calling DataPump via HTTP
http://yourwebserver/DataPump/Json.aspx?connection='connection name'&procedure='stored procedure name'&startdate=01/01/2012	

The web service will check the connections names against the whitelist and when a match occurs, execute
the stored procedure.			
				