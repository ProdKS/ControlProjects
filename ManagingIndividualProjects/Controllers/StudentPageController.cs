using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
        public IActionResult AddProject()
        {
            // Создаем список элементов для выпадающего списка
            var dataList = await workBD.Users.Where(x => x.Role == 1 && x.GroupDep == GroupInt).ToListAsync()
            {
                new SelectListItem { Text = "Опция 1", Value = "1" },
                new SelectListItem { Text = "Опция 2", Value = "2" },
                new SelectListItem { Text = "Опция 3", Value = "3" },
                new SelectListItem { Text = "Опция 4", Value = "4" }
            };
            ViewBag.DataList = dataList;

            return View();
        }

        [HttpPost]
        public IActionResult AddProject(ModelForAddingProjects addingProjects)
        {
            // Здесь вы можете обработать данные формы, если это необходимо
            return RedirectToAction("StudentPage");
        }
    }
}
