using Microsoft.AspNetCore.Mvc;

namespace ManagingIndividualProjects.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
