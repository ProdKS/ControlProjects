using Microsoft.AspNetCore.Mvc;

namespace ManagingIndividualProjects.Controllers
{
    public class TeacherController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
