using BCrypt.Net;
using System;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using ThinkBridge.Helpers;
using ThinkBridge.Models;
using ThinkBridge.ViewModels;

namespace ThinkBridge.Controllers
{
    [CustomAuthorize("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Dashboard()
        {
            var totalUsers = _db.Users.Count();
            var totalStudents = _db.Users.Count(u => u.Role == "Student");
            var totalLecturers = _db.Users.Count(u => u.Role == "Lecturer");
            var recentUsers = _db.Users.OrderByDescending(u => u.CreatedAt).Take(10).ToList();
            var allSubjects = _db.Subjects.OrderBy(s => s.Name).ToList();
            var recentActivity = _db.ProgressRecords.OrderByDescending(p => p.CompletedAt).Take(20).ToList();

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalStudents = totalStudents,
                TotalLecturers = totalLecturers,
                RecentUsers = recentUsers,
                AllSubjects = allSubjects,
                RecentActivity = recentActivity
            };

            return View(viewModel);
        }

        public ActionResult Users()
        {
            var users = _db.Users.OrderBy(u => u.Role).ThenBy(u => u.FullName).ToList();
            return View(users);
        }

        public ActionResult CreateLecturer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateLecturer(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_db.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                return View(model);
            }

            var lecturer = new User
            {
                Username = model.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Email = model.Email,
                FullName = model.FullName,
                Role = "Lecturer",
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _db.Users.Add(lecturer);
            _db.SaveChanges();

            TempData["Success"] = "Lecturer account created successfully!";
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleUserStatus(int id)
        {
            var user = _db.Users.Find(id);
            if (user != null && user.Role != "Admin")
            {
                user.IsActive = !user.IsActive;
                _db.SaveChanges();
                TempData["Success"] = $"User {user.Username} status updated.";
            }
            return RedirectToAction("Users");
        }

        public ActionResult Subjects()
        {
            var subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
            return View(subjects);
        }

        public ActionResult SystemActivity()
        {
            var activities = _db.ProgressRecords
                .OrderByDescending(p => p.CompletedAt)
                .Take(50)
                .ToList();
            return View(activities);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}