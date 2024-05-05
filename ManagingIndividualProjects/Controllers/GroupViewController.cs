using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagingIndividualProjects.Controllers
{
    public class GroupViewController : Controller
    {
        private ProjectsContext workBD;
        public GroupViewController(ProjectsContext workbd)
        {
            this.workBD = workbd;
        }
        public IActionResult GroupView()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var groups = workBD.Groups.ToList();
            return View(groups);
        }
        public async Task<IActionResult> GroupDetailView(long groupsId)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var group = await workBD.Groups.FindAsync(groupsId);
            if (group == null)
            {
                return RedirectToAction("GroupView");
            }            
            var model = new UserDetail();
            model.NameGroup = group.Name;
            model.Users = await workBD.Users.Where(x => x.GroupDep == groupsId).ToListAsync();
            return View(model);
        }
    }
}
