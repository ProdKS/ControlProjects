using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ManagingIndividualProjects.Controllers
{
    public class ZVPageController : Controller
    {
        private ProjectsContext workBD;
        public ZVPageController(ProjectsContext workbd)
        {
            this.workBD = workbd;
        }
        public async Task<IActionResult> ZVPage()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            int Departament = Convert.ToInt32(HttpContext.Session.GetString("Departament"));
            var groups = await workBD.Groups.Where(x => x.DepartmentId == Departament).ToListAsync();
            return View(groups);
        }
        public async Task<IActionResult> GroupDetailView(long groupsId)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var group = await workBD.Groups.FindAsync(groupsId);
            if (group == null)
            {
                return RedirectToAction("ZVPage");
            }
            var model = new UserDetail();
            model.NameGroup = group.Name;
            model.Users = await workBD.Users.Where(x => x.GroupDep == groupsId && x.Role == 1).ToListAsync();
            return View(model);
        }
    }
}
