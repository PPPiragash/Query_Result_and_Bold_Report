using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Web.Optimization;


namespace Query_Result.Controllers
{
    [RoutePrefix("query")]
    public class QueryController : ApiController
    {
        private const string ApiUrl = "https://ts4.optimo.training/V4.5.0/RestAPI/OLE/api/v4.2/reports/query/51?include=QueryResult&executeQuery=true&queryResultsPageSize=100";
        private const string Token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6Im9wdGltbyIsIklEIjoiMiIsIkV4cGlyZURhdGUiOiI0LzE4LzIwMjUgOTo1Mjo1OCBBTSIsImlzcyI6Im9wdGltbyIsIkFwcGxpY2F0aW9uSWQiOiIiLCJTYWxlc0NoYW5uZWxJZCI6IiIsIm5iZiI6MTc0NDM2NTE3OCwiZXhwIjoxNzQ0OTY5OTc4LCJpYXQiOjE3NDQzNjUxNzh9.fcP8OK2Yb3-a8cupdP0pPsBJv6uk_YtC5wxgtmimX3meJTNA00ffC__cttHq1d8fBD5dnwRP0Nqp4ildcdSKgfnVVGBc8feEZYPDspzqqpC0Yp8TmwZhkNafcYAKC0L4TwpOBDDFeqR-6DJoilrBSfn_ljEpcTgtLrvg0UWEZszIDMto2buts7eLfW9AnVwhp0Gb2srcvDnQyWNd6YmTClOXnvjY_1bKIDl9KAuy90jdLg3qFjNX4_ySLOQjsbCs1npr3NQpJQCLpoUGn12KMsaoWIpIp4dlRAFf3-SEGlFv6LraEngh6STznJxQU0s0xT2qJFQAGRFtxMW7QmtqmA";

        [HttpGet]
        [Route("GetCleanQueryResults")]
        public async Task<IHttpActionResult> GetCleanQueryResults()
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, ApiUrl);
                request.Headers.Add("token", Token);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return Content(response.StatusCode, "API call failed.");
                }

                var content = await response.Content.ReadAsStringAsync();

                try
                {
                    var json = JObject.Parse(content);
                    var included = json["included"] as JArray;

                    if (included == null)
                        return Content(HttpStatusCode.InternalServerError, "Missing 'included' section.");

                    var queryResults = included
                        .Where(x => (string)x["type"] == "queryResult")
                        .Select(x => x["attributes"]["queryResults"])
                        .ToList();

                    return Json(queryResults);
                }
                catch (Exception ex)
                {
                    return Content(HttpStatusCode.InternalServerError, "Error processing response: " + ex.Message);
                }
            }
        }
    }
}
