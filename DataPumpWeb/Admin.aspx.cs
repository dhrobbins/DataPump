using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.Configuration;
using DataPump;

namespace DataPumpWeb
{
    public partial class Admin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            LoadSourceTargetData();
        }

        private void LoadSourceTargetData()
        {
            string connection = ConfigurationManager.ConnectionStrings["DataPump"].ConnectionString;
            string procedure = ConfigurationManager.AppSettings["GetSourceTargets"].ToString();

            var sourceTargetManager = new SourceTargetManager(connection, procedure);
            var sourceTargets = sourceTargetManager.GetSourceTargets();

            //  update ImageRef so UI can display some image
            sourceTargets.ForEach(x => x.ImageRef = @"images/plus.jpg");

            this.sourceTargetData.Value = JsonConvert.SerializeObject(sourceTargets);
        }
    }
}