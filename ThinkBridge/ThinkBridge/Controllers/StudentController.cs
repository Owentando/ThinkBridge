using System.Web.Mvc;
using ThinkBridge.Helpers;

namespace ThinkBridge.Controllers
{
    [CustomAuthorize("Student")]
    public class StudentController : Controller
    {
        public ActionResult Dashboard()
        {
            return View();
        }
    }
}