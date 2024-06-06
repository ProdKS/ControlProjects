using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ManagingIndividualProjects.Controllers
{
    public class TeacherPageController : Controller
    {
        private ProjectsContext workBD;
        public TeacherPageController(ProjectsContext workbd)
        {
            this.workBD = workbd;
        }
        public async Task<IActionResult> TeacherPageAsync()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            int nowUserId = Convert.ToInt32(HttpContext.Session.GetString("Id"));
            var hasClassroom = await workBD.Groups.AnyAsync(x => x.ClassroomTeacher == nowUserId);
            var subjects = await workBD.Subjects.ToListAsync();
            var students = await workBD.Students.ToListAsync();
            
            var takeSubjects = await workBD.Subjects.Where(x => x.Teacherid == nowUserId).ToListAsync();
            List<IndividualProject> individualProjectsList = new List<IndividualProject>();
            foreach (var subject in takeSubjects)
            {
                var individualProjects = await workBD.IndividualProjects.Where(x => x.Subject == subject.Id).ToListAsync();
                if (individualProjects.Any())
                {
                    individualProjectsList.AddRange(individualProjects);
                }
            }
            var model = new TeacherProjectsModel
            {
                IndividualProjects = individualProjectsList,
                Students = students,
                IsClassroom = hasClassroom,
                Subjects = subjects,
            };
            return View(model);
        }
        public async Task<IActionResult> AcceptProject(int IndividualProjectid)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var projectToUpdate = await workBD.IndividualProjects.FindAsync(IndividualProjectid);
            if (projectToUpdate != null)
            {
                projectToUpdate.Status = 1;
                await workBD.SaveChangesAsync();
            }
            return RedirectToAction("TeacherPage", "TeacherPage");
        }
        public async Task<IActionResult> DeclineProject(int IndividualProjectid)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var projectToUpdate = await workBD.IndividualProjects.FindAsync(IndividualProjectid);
            if (projectToUpdate != null)
            {
                projectToUpdate.Status = 2;
                await workBD.SaveChangesAsync();
            }
            return RedirectToAction("TeacherPage", "TeacherPage");
        }
        public async Task<IActionResult> EditProjectTheme(int IndividualProjectid)
        {
            return View();
        }
    }
}
