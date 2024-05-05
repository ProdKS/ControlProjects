using Microsoft.AspNetCore.Mvc;

namespace ManagingIndividualProjects.Controllers
{
    public class StudentPageController : Controller
    {
        public IActionResult StudentPage()
        {
            return View();
        }
    }
}
