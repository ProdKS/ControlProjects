using Microsoft.AspNetCore.Mvc;

namespace ManagingIndividualProjects.Controllers
{
    public class TeacherPageController : Controller
    {
        public IActionResult TeacherPage()
        {
            return View();
        }
    }
}
