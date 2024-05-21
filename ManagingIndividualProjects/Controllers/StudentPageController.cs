using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagingIndividualProjects.Controllers
{
    public class StudentPageController : Controller
    {
        private ProjectsContext workBD;
        public StudentPageController(ProjectsContext workbd)
        {
            this.workBD = workbd;

        }
        public async Task<IActionResult> StudentPage()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var readyTheme = HttpContext.Session.GetString("Theme");
            int idStudent = Convert.ToInt32(HttpContext.Session.GetString("Id"));
            var individualProject = await workBD.IndividualProjects.Where(x => x.Student == idStudent).ToListAsync();          
            if (readyTheme == null)
            {

                return View();
            }
            else
            {
                var model = new IndividualProjectModel
                {
                    IndividualProjects = individualProject
                };
                return View(model);
            }            
        }
    }
}
