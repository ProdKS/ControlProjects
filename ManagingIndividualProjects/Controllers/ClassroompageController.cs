using Microsoft.AspNetCore.Mvc;

namespace ManagingIndividualProjects.Controllers
{
    public class ClassroompageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
