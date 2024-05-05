using Microsoft.AspNetCore.Mvc;

namespace ManagingIndividualProjects.Controllers
{
    public class StudentViewController : Controller
    {
        public IActionResult StudentView()
        {
            return View();
        }
    }
}
