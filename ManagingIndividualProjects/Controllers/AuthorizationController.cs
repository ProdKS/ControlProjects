using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagingIndividualProjects.Controllers
{
    public class AuthorizationController : Controller
    {

        private ProjectsContext workBD;
        public AuthorizationController(ProjectsContext workbd)
        {
            this.workBD = workbd;
        }
        [HttpGet]
        public IActionResult Authorization()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Authorization(ModelForAuth authorization)
        {
            Student nowStudent = await workBD.Students.FirstOrDefaultAsync(x => x.Login == authorization.userName && x.Password == authorization.password);
            Employee nowEmployee = await workBD.Employees.FirstOrDefaultAsync(x => x.Login == authorization.userName && x.Password == authorization.password);
            if (nowStudent != null)
            {
                try
                {
                    HttpContext.Session.SetString("Id", nowStudent.Id.ToString());
                }
                catch (Exception ex)
                {

                }             
                try
                {
                    HttpContext.Session.SetString("Login", nowStudent.Login);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    HttpContext.Session.SetString("Password", nowStudent.Password);
                }
                catch (Exception ex)
                {
                    
                }
                try
                {
                    HttpContext.Session.SetString("Surname", nowStudent.Surname);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    HttpContext.Session.SetString("Name", nowStudent.Name);
                }
                catch (Exception ex)
                {
                    
                }
                try
                {
                    HttpContext.Session.SetString("Pat", nowStudent.Pat);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    HttpContext.Session.SetString("Number", nowStudent.Number);
                }
                catch (Exception ex)
                {
                    
                }
                try
                {
                    HttpContext.Session.SetString("Role", nowStudent.Role.ToString());
                }
                catch (Exception ex)
                {

                }
                try
                {
                    HttpContext.Session.SetString("Group", nowStudent.GroupDep.ToString());
                }
                catch (Exception ex)
                {

                }                            
                if(nowStudent.Role == 1)
                {
                    return RedirectToAction("StudentPage", "StudentPage");
                }                               
            }
            else if (nowEmployee != null)
            {
                try
                {
                    HttpContext.Session.SetString("Id", nowEmployee.Id.ToString());
                }
                catch
                {

                }
                try
                {
                    HttpContext.Session.SetString("Login", nowEmployee.Login);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    HttpContext.Session.SetString("Password", nowEmployee.Password);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    HttpContext.Session.SetString("Surname", nowEmployee.Surname);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    HttpContext.Session.SetString("Name", nowEmployee.Name);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    HttpContext.Session.SetString("Pat", nowEmployee.Pat);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    HttpContext.Session.SetString("Number", nowEmployee.Number);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    HttpContext.Session.SetString("Role", nowEmployee.Role.ToString());
                }
                catch (Exception ex)
                {

                }
                try
                {
                    HttpContext.Session.SetString("DepartmentEmployee", nowEmployee.GroupDep.ToString());
                }
                catch (Exception ex)
                {

                }
                if (nowEmployee.Role == 2)
                {
                    return RedirectToAction("ClassroomPage", "ClassroomPage");
                }
                else if (nowEmployee.Role == 3)
                {
                    return RedirectToAction("TeacherPage", "TeacherPage");
                }
                else if (nowEmployee.Role == 4)
                {
                    return RedirectToAction("ZVPage", "ZVPage");
                }
                else if (nowEmployee.Role == 5)
                {
                    return RedirectToAction("DepartmentListView", "DepartmentPage");
                }
            }
           return View();
        }
    }
}
