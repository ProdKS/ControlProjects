using Microsoft.AspNetCore.Mvc;

namespace ManagingIndividualProjects.Controllers
{
    public class ClassroomPageController : Controller
    {
        public IActionResult ClassroomPage()
        {
            return View();
        }
    }
}
