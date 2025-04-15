using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using BoldReports.Web.ReportViewer;
using BoldReports.Writer;
using System.Web.Caching;
using System.Data;
using System.Web.Http;
using System.Web.Optimization;

using System.Net.Http;

namespace Query_Result.Controllers
{
  
public class ReportViewerController : Controller, IReportController
    {
        private readonly Cache _cache;
        private const string ResourceFolder = "Resources";

        public ReportViewerController()
        {
            _cache = HttpContext.Cache;
        }

        [System.Web.Http.HttpGet]
        public ActionResult PostReportAction([FromBody] Dictionary<string, object> jsonArray)
        {
            var result = ReportHelper.ProcessReport(jsonArray, this);
            return Json(result);
        }

        [System.Web.Mvc.NonAction]
        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            string reportPath = Path.Combine(GetResourcePath(), reportOption.ReportModel.ReportPath + ".rdl");
            if (!System.IO.File.Exists(reportPath))
            {
                throw new FileNotFoundException("Report file not found: " + reportPath);
            }
            using (FileStream inputStream = new FileStream(reportPath, FileMode.Open, FileAccess.Read))
            {
                MemoryStream reportStream = new MemoryStream();
                inputStream.CopyTo(reportStream);
                reportStream.Position = 0;
                reportOption.ReportModel.Stream = reportStream;
            }
        }

        [System.Web.Mvc.NonAction]
        public void OnReportLoaded(ReportViewerOptions reportOption)
        {
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.ActionName("GetResource")]
        public ActionResult GetResource(ReportResource resource)
        {
            string resourceKey = resource?.key ?? string.Empty;
            string resourceType = "defaultResourceType";
            bool isPrint = false;
            var result = ReportHelper.GetResource(resourceKey, resourceType, isPrint);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Http.HttpPost]
        public ActionResult PostFormReportAction()
        {
            var result = ReportHelper.ProcessReport(null, this);
            return Json(result);
        }

        private string GetResourcePath()
        {
            return Path.Combine(HttpContext.Server.MapPath("~/wwwroot/"), ResourceFolder);
        }

        object IReportController.PostReportAction(Dictionary<string, object> jsonResult)
        {
            return ReportHelper.ProcessReport(jsonResult, this);
        }

        public object GetResource(string key, string resourcetype, bool isPrint)
        {
            return ReportHelper.GetResource(key, resourcetype, isPrint);
        }
    }


}
