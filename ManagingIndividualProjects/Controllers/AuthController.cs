using Microsoft.AspNetCore.Mvc;

namespace ManagingIndividualProjects.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Authorization()
        {
            return View();
        }
    }
}
