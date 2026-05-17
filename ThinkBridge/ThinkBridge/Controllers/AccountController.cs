using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using ThinkBridge.Models;
using ThinkBridge.ViewModels;

namespace ThinkBridge.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (Session["UserId"] != null)
                return RedirectToRoleDashboard();
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _db.Users.FirstOrDefault(u => u.Username == model.Username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            Session["UserId"] = user.Id;
            Session["Username"] = user.Username;
            Session["Role"] = user.Role;
            Session["DisplayName"] = user.FullName;

            return RedirectToRoleDashboard();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (Session["UserId"] != null)
                return RedirectToRoleDashboard();
            return View(new RegisterViewModel());
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // DUT student email format only: 8 digits followed by @dut4life.ac.za
            var dutEmailPattern = @"^\d{8}@dut4life\.ac\.za$";

            if (string.IsNullOrWhiteSpace(model.Email) ||
                !Regex.IsMatch(model.Email.Trim(), dutEmailPattern, RegexOptions.IgnoreCase))
            {
                ModelState.AddModelError("Email", "Only DUT student emails are allowed. Example: 22304400@dut4life.ac.za");
                return View(model);
            }

            if (_db.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                return View(model);
            }

            if (_db.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already registered.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Email = model.Email.Trim(),
                FullName = model.FullName,
                Role = "Student",
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            Session["UserId"] = user.Id;
            Session["Username"] = user.Username;
            Session["Role"] = user.Role;
            Session["DisplayName"] = user.FullName;

            return RedirectToAction("Dashboard", "Student");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public ActionResult Unauthorized()
        {
            return View();
        }

        private ActionResult RedirectToRoleDashboard()
        {
            var role = Session["Role"]?.ToString();
            switch (role)
            {
                case "Admin": return RedirectToAction("Dashboard", "Admin");
                case "Lecturer": return RedirectToAction("Dashboard", "Student");
                case "Student": return RedirectToAction("Dashboard", "Student");
                default: return RedirectToAction("Login");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}