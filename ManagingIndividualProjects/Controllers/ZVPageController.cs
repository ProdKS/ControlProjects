using Microsoft.AspNetCore.Mvc;

namespace ManagingIndividualProjects.Controllers
{
    public class ZVPageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
