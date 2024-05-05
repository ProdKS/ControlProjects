using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagingIndividualProjects.Controllers
{
    public class AuthorizationController : Controller
    {
        private ProjectsContext workBD;
        public AuthorizationController(ProjectsContext workbd)
        {
            this.workBD = workbd;
        }
        [HttpGet]
        public IActionResult Authorization()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Authorization(ModelForAuth authorization)
        {
            User nowUser = await workBD.Users.FirstOrDefaultAsync(x => x.Login == authorization.userName && x.Password == authorization.password);
            if (nowUser != null)
            {
                HttpContext.Session.SetString("Id", nowUser.Id.ToString());
                HttpContext.Session.SetString("Login", nowUser.Login);
                HttpContext.Session.SetString("Password", nowUser.Password);
                HttpContext.Session.SetString("Surname", nowUser.Surname);
                HttpContext.Session.SetString("Name&Pat", nowUser.NameAndPat);
                HttpContext.Session.SetString("Number", nowUser.Number);
                HttpContext.Session.SetString("Role", nowUser.Role.ToString());
                HttpContext.Session.SetString("Theme", nowUser.Theme.ToString());
                HttpContext.Session.SetString("Group", nowUser.GroupDep.ToString());
                HttpContext.Session.SetString("Departament", nowUser.Department.ToString());
                // 1 - student, 2 - classroom, 3 - teacher, 4 - zvotdel, 5 - sotr y4 chasti
                if(nowUser.Role == 1)
                {

                }
                else if (nowUser.Role == 2)
                {

                }
                else if(nowUser.Role == 3) 
                { 
                
                } else if (nowUser.Role == 4)
                {

                } else if(nowUser.Role == 5) 
                {
                    return RedirectToAction("DepartmentListView", "DepartmentPage");
                }                
            }
           return View();
        }
    }
}
