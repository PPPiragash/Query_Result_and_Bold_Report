using System.IO;
using System.Web.Mvc;

namespace Query_Result.Controllers
{
    internal class ErrorViewModel : IView
    {
        public string RequestId { get; set; }

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}