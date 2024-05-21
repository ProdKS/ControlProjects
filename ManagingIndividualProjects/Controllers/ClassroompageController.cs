using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagingIndividualProjects.Controllers
{
    public class ClassroomPageController : Controller
    {
        private ProjectsContext workBD;
        public ClassroomPageController(ProjectsContext workbd)
        {
            this.workBD = workbd;
        }
        public async Task<IActionResult> ClassroomPage()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            int GroupInt = Convert.ToInt32(HttpContext.Session.GetString("Group"));
            var students = await workBD.Users.Where(x => x.Role == 1 && x.GroupDep == GroupInt).ToListAsync();
            var model = new StudentViewModel
            {
                Students = students
            };
            return View(model);
        }
    }
}
