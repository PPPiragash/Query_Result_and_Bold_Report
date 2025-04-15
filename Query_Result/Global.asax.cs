using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
using System.Configuration;
using Bold.Licensing;
using BoldReports.Web;

namespace Query_Result
{
    public class Global : System.Web.HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // 🔐 Read Bold Reports license key from Web.config
            string licenseKey = ConfigurationManager.AppSettings["BoldLicense"];
            BoldLicenseProvider.RegisterLicense(licenseKey);

            // 🔌 Register Web API data extension for Bold Reports
            ReportConfig.DefaultSettings = new ReportSettings()
                .RegisterExtensions(new List<string>
                {
                    "BoldReports.Data.WebData",
                    "BoldReports.Data.SQL",
                    "BoldReports.Data.Json"
                });


            // 🛠 Register routes and Web API config
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
