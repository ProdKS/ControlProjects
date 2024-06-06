using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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
            int NowUserId = Convert.ToInt32(HttpContext.Session.GetString("Id"));
            int GroupInt = Convert.ToInt32(HttpContext.Session.GetString("Group"));
            var groups = await workBD.Groups.Where(x => x.ClassroomTeacher == NowUserId).ToListAsync();
            var departments = new List<Department>();
            if (groups == null)
            {
                return RedirectToAction("ClassroomPage");
            }
            foreach (var group in groups)
            {
                if (group.IsDepartment.HasValue)
                {
                    var department = await workBD.Departments.FirstOrDefaultAsync(d => d.Id == group.DepartmentId.Value && group.IsDepartment > 6);
                    if (department != null)
                    {
                        departments.Add(department);
                    }
                }
            }

            var model = new GroupsForClassroom
            {
                Groups = groups,
                Departments = departments
            };

            return View(model);
        }
        public async Task<IActionResult> ClassroomPageView(int groupID)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var group = await workBD.Groups.FindAsync(groupID);
            var individualProjects = await workBD.IndividualProjects.ToListAsync();
            if (group == null)
            {
                return RedirectToAction("ClassroomPage");
            }
            var model = new StudentsView();
            model.IndividualProjects = individualProjects;
            model.nameGroup = group.Name;
            model.Students = await workBD.Students.Where(x => x.GroupDep == groupID && x.Role == 1).ToListAsync();
            return View(model);
        }
    }
}
