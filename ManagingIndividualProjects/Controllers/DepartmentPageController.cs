using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagingIndividualProjects.Controllers
{
    public class DepartmentPageController : Controller
    {
        private ProjectsContext workBD;
        public DepartmentPageController(ProjectsContext workbd)
        {
            this.workBD = workbd;
        }
        public IActionResult DepartmentListView()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var departments = workBD.Departments.ToList();
            return View(departments);
        }
        public async Task<IActionResult> DepartmentDetailView(long departmentId)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var departments = await workBD.Departments.FindAsync(departmentId);
            if (departments == null)
            {
                return RedirectToAction("DepartmentListView");
            }
            var model = new DepartmentDetail();
            model.NameDepartment = departments.Name;
            model.Groups = await workBD.Groups.Where(x => x.DepartmentId == departmentId).ToListAsync();
            return View(model);
        }
        public async Task<IActionResult> GroupDetailView(long groupid)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var group = await workBD.Groups.FindAsync(groupid);
            if (group == null)
            {
                return RedirectToAction("DepartmentListView");
            }
            var model = new UserDetail();
            model.NameGroup = group.Name;
            model.Users = await workBD.Users.Where(x => x.GroupDep == groupid).ToListAsync();
            return View(model);
        }
    }
}
