using Microsoft.AspNetCore.Mvc;

namespace ManagingIndividualProjects.Controllers
{
    public class ZVPageController : Controller
    {
        public IActionResult ZVPage()
        {
            return View();
        }
    }
}
