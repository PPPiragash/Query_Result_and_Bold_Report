using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using BoldReports.Web.ReportDesigner;
using BoldReports.Web.ReportViewer;
using BoldReports;
using System.Web.Hosting;
using System.Web.Optimization;

namespace ReportViewer.Controllers
{
    [RoutePrefix("api/ReportDesigner")]
    public class ReportDesignerController : ApiController, IReportController , IReportDesignerController
    {
        private const string ResourceFolder = "Resources";

        private string GetResourcePath()
        {
            return HttpContext.Current.Server.MapPath("~/" + ResourceFolder);

        }

        [HttpPost]
        [Route("PostDesignerAction")]
        public HttpResponseMessage PostDesignerAction([FromBody] Dictionary<string, object> jsonResult)
        {
            try
            {
                if (jsonResult.ContainsKey("DataSource"))
                {
                    var dataSource = jsonResult["DataSource"]?.ToString();
                    if (!string.IsNullOrEmpty(dataSource) && !dataSource.Contains("WebAPI"))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { error = "Only predefined WebAPI data source is allowed." });
                    }
                }


                var result = ReportDesignerHelper.ProcessDesigner(jsonResult, (IReportDesignerController)this, null);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    error = "Designer action failed",
                    details = ex.Message
                });
            }
        }


        [HttpPost]
        [Route("PostFormDesignerAction")]
        public HttpResponseMessage PostFormDesignerAction()
        {
            try
            {
                var result = ReportDesignerHelper.ProcessDesigner(null, (IReportDesignerController)this, null);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { error = "Form designer action failed", details = ex.Message });
            }
        }

        [HttpPost]
        [Route("UploadReportAction")]
        public HttpResponseMessage UploadReportAction()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Files.Count == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No files uploaded.");

                var file = httpRequest.Files[0];
                var uploadPath = GetResourcePath();

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, file.FileName);
                file.SaveAs(filePath);

                return Request.CreateResponse(HttpStatusCode.OK, "File uploaded successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { error = "File upload failed", details = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetImage")]
        public HttpResponseMessage GetImage(string key, string image)
        {
            try
            {
                string imagePath = Path.Combine(GetResourcePath(), image);
                if (!File.Exists(imagePath))
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(File.ReadAllBytes(imagePath))
                };
                result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                return result;
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { error = "Failed to retrieve image", details = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetReports")]
        public HttpResponseMessage GetReports()
        {
            try
            {
                string path = GetResourcePath();

                if (!Directory.Exists(path))
                    return Request.CreateResponse(HttpStatusCode.OK, new List<string>());

                var reportFiles = Directory.GetFiles(path, "*.rdl")
                                           .Select(Path.GetFileNameWithoutExtension)
                                           .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, reportFiles);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { error = "Failed to retrieve reports", details = ex.Message });
            }
        }

        [HttpDelete]
        [Route("DeleteReport")]
        public HttpResponseMessage DeleteReport(string reportName)
        {
            try
            {
                string reportPath = Path.Combine(GetResourcePath(), reportName + ".rdl");

                if (File.Exists(reportPath))
                {
                    File.Delete(reportPath);
                    return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Report deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.NotFound, "Report not found");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { error = "Failed to delete report", details = ex.Message });
            }
        }

        // Renamed to avoid conflict with the interface method
        [HttpPost]
        [Route("PostReportAction")]
        public HttpResponseMessage PostReportActionFromApi([FromBody] Dictionary<string, object> jsonResult)
        {
            try
            {
                if (jsonResult.TryGetValue("reportPath", out var reportName))
                {
                    var reportPath = Path.Combine(GetResourcePath(), reportName.ToString() + ".rdl");
                    if (!File.Exists(reportPath))
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, new { error = "Report not found" });
                    }
                }

                var result = ReportHelper.ProcessReport(jsonResult, this);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { error = "Preview failed", details = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetDataSources")]
        public HttpResponseMessage GetDataSources()
        {
            try
            {
                var dataSources = new List<Dictionary<string, object>>();

                var webApiDataSource = new Dictionary<string, object>
                {
                    { "Name", "PredefinedWebAPI" },
                    { "DataSourceType", "RESTAPI" },
                    { "ConnectionProperties", new Dictionary<string, object>
                        {
                            { "DataProvider", "RESTAPI" },
                            { "ConnectString", "https://ts4.optimo.training/V4.5.0/RestAPI/OLE/api/v4.2/reports/query/51?include=QueryObjectFieldMapping,QueryUserGroup,QueryFilterGroup,QueryResultsFilter,QueryResult&executeQuery=true&queryResultsPageSize=100" }
                        }
                    }
                };

                dataSources.Add(webApiDataSource);

                return Request.CreateResponse(HttpStatusCode.OK, dataSources);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { error = "Failed to retrieve data sources", details = ex.Message });
            }
        }

        // Implementing the IReportController interface methods
        public object PostReportAction(Dictionary<string, object> jsonResult)
        {
            // Implementing PostReportAction logic here
            throw new NotImplementedException("PostReportAction is not implemented yet.");
        }

        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            // Implement initialization logic here for report options if necessary
            throw new NotImplementedException("OnInitReportOptions is not implemented yet.");
        }

        public void OnReportLoaded(ReportViewerOptions reportOption)
        {
            // Implement logic when the report is loaded if necessary
            throw new NotImplementedException("OnReportLoaded is not implemented yet.");
        }

        public object GetResource(string key, string resourcetype, bool isPrint)
        {
            // Implement resource retrieval logic based on key and type
            throw new NotImplementedException("GetResource is not implemented yet.");
        }

        object IReportDesignerController.PostDesignerAction(Dictionary<string, object> jsonResult)
        {
            throw new NotImplementedException();
        }

        void IReportDesignerController.UploadReportAction()
        {
            throw new NotImplementedException();
        }

        object IReportDesignerController.GetImage(string key, string image)
        {
            throw new NotImplementedException();
        }

        public bool SetData(string key, string itemId, ItemInfo itemData, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                // Implement your logic to set data here
                // For example, save itemData to a database or file system
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }




















































       public ResourceInfo GetData(string key, string itemId)
        {
            return new ResourceInfo();
        }

    }
}