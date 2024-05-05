using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagingIndividualProjects.Controllers
{
    public class StudentViewController : Controller
    {
        private ProjectsContext workBD;
        public StudentViewController(ProjectsContext workbd)
        {
            this.workBD = workbd;

        }
        public async Task<IActionResult> StudentView()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var students = await workBD.Users.Where(x => x.Role == 1).ToListAsync();
            var model = new StudentViewModel
            {
                Students = students
            };
            return View(model);
        }
    }
}
