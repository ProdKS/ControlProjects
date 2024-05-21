using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagingIndividualProjects.Controllers
{
    public class StudentViewEmployeController : Controller
    {
        private ProjectsContext workBD;
        public StudentViewEmployeController(ProjectsContext workbd)
        {
            this.workBD = workbd;

        }
        public async Task<IActionResult> StudentViewEmploye()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            int DepartmentInt = Convert.ToInt32(HttpContext.Session.GetString("Departament"));
            var students = await workBD.Users.Where(x => x.Role == 1 && x.Department == DepartmentInt).ToListAsync();
            var model = new StudentViewModel
            {
                Students = students
            };
            return View(model);
        }
    }
}
