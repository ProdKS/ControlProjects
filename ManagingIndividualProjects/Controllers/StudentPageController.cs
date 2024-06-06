using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;

namespace ManagingIndividualProjects.Controllers
{
    public class StudentPageController : Controller
    {
        private ProjectsContext workBD;
        public StudentPageController(ProjectsContext workbd)
        {
            this.workBD = workbd;

        }
        public async Task<IActionResult> StudentPage()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            int idStudent = Convert.ToInt32(HttpContext.Session.GetString("Id"));           
            var individualProject = await workBD.IndividualProjects.Where(x => x.Student == idStudent).ToListAsync();
            var listSubjects = workBD.Subjects.ToList();
            var listEmployees = workBD.Employees.ToList();
            if (individualProject == null)
            {
                return View();
            }
            else
            {
                workBD.Subjects.Include(x => x.Teacher);
                var model = new IndividualProjectModel
                {
                    Subjects = listSubjects,
                    IndividualProjects = individualProject
                };
                return View(model);
            }            
        }
        public IActionResult AddProject()
        {
            int groupStudent = Convert.ToInt32(HttpContext.Session.GetString("Group"));
            var teacherIDs = (from eg in workBD.EmployeeGroups
                              where eg.GroupId == groupStudent
                              select eg.TeacherId).ToList();
            var subjectInfos = (from s in workBD.Subjects
                                join t in workBD.Employees on s.Teacherid equals t.Id
                                where teacherIDs.Contains(s.Teacherid)
                                select new
                                {
                                    SubjectID = s.Id,
                                    SubjectName = s.Name,
                                    TeacherFullName = t.Surname + " " + t.Name + " " + t.Pat
                                }).ToList();
            
            var model = new IndividualProjectModel
            {
                
                SubjectOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Выберите..." }
            }
            };
            foreach (var info in subjectInfos)
            {
                string result = $"{info.SubjectName} ({info.TeacherFullName})";
                model.SubjectOptions.Add(new SelectListItem { Value = info.SubjectID.ToString(), Text = result });
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddProjectAsync(IndividualProjectModel addingProjects)
        {
            int idStudent = Convert.ToInt32(HttpContext.Session.GetString("Id"));        
            var project = new IndividualProject
            {
                NameTheme = addingProjects.nameTheme,
                Subject = addingProjects.SelectedOption,
                Student = idStudent,
                Status = 3,
                Gradle = null,
                Feedback = null
            };
            workBD.IndividualProjects.Add(project);
            await workBD.SaveChangesAsync();
            return RedirectToAction("StudentPage");                        
        }
        public async Task<IActionResult> AboutFeedback(int idProject)
        {
            var individualProject = await workBD.IndividualProjects.FindAsync(idProject);
            string gradle = "";
            string status = "";
            if (individualProject == null)
            {
                return RedirectToAction("StudentPage");
            }
            if (individualProject.Gradle == null)
            {
                gradle = "Отсутствует";
            }
            else
            {
                gradle = individualProject.Gradle.ToString();
            }
            if (individualProject.Status == 1)
            {
                status = "Одобрено";
            }
            else if(individualProject.Status == 2)
            {
                status = "Отклонена";
            }
            else if (individualProject.Status == 3)
            {
                status = "Не просмотрено";
            }
            else if(individualProject.Status == 4)
            {
                status = "Оценено";
            }
            var model = new FeedbackViewModel();
            model.status = status;
            model.gradle = gradle;
            model.IndividualProject = individualProject;
            return View(model);
        }
        public async Task<IActionResult> AcceptChangedTheme(int projectID)
        {
            var projectToUpdate = await workBD.IndividualProjects.FindAsync(projectID);
            if (projectToUpdate != null)
            {
                projectToUpdate.Status = 1;
                await workBD.SaveChangesAsync();
            }
            return RedirectToAction("StudentPage", "StudentPage");
        }
        public async Task<IActionResult> DeclineChangedTheme(int projectID)
        {
            var projectToUpdate = await workBD.IndividualProjects.FindAsync(projectID);
            if (projectToUpdate != null)
            {
                projectToUpdate.Status = 1;
                await workBD.SaveChangesAsync();
            }
            return RedirectToAction("StudentPage", "StudentPage");
        }
        public async Task<IActionResult> DeleteProject(int projectID)
        {
            var projectToRemove = await workBD.IndividualProjects.FindAsync(projectID);
            if (projectToRemove != null)
            {
                workBD.IndividualProjects.Remove(projectToRemove);
                await workBD.SaveChangesAsync();
            }
            return RedirectToAction("StudentPage", "StudentPage");

        }
    }
}
