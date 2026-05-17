using System.Web.Mvc;

namespace ThinkBridge.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserId"] != null)
            {
                var role = Session["Role"]?.ToString();
                switch (role)
                {
                    case "Admin": return RedirectToAction("Dashboard", "Admin");
                    case "Lecturer": return RedirectToAction("Dashboard", "Lecturer");
                    case "Student": return RedirectToAction("Dashboard", "Student");
                }
            }
            return RedirectToAction("Login", "Account");
        }
    }
}