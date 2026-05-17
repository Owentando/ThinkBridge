using System.Web.Mvc;
using ThinkBridge.Helpers;

namespace ThinkBridge.Controllers
{
    [CustomAuthorize("Lecturer")]
    public class LecturerController : Controller
    {
        public ActionResult Dashboard()
        {
            return View();
        }
    }
}