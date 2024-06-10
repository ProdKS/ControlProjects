using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ManagingIndividualProjects.Controllers
{
    public class ClassroomPageController : Controller
    {
        private ProjectsContext workBD;
        public ClassroomPageController(ProjectsContext workbd)
        {
            this.workBD = workbd;
        }
        public async Task<IActionResult> ClassroomPage()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            int NowUserId = Convert.ToInt32(HttpContext.Session.GetString("Id"));
            var groups = await workBD.Groups.Where(x => x.ClassroomTeacher == NowUserId).ToListAsync();
            var departments = new List<Department>();
            if (groups == null)
            {
                return RedirectToAction("ClassroomPage");
            }
            foreach (var group in groups)
            {
                if (group.IsDepartment.HasValue)
                {
                    var department = await workBD.Departments.FirstOrDefaultAsync(d => d.Id == group.DepartmentId.Value && group.IsDepartment > 6);
                    if (department != null)
                    {
                        departments.Add(department);
                    }
                }
            }

            var model = new GroupsForClassroom
            {
                Groups = groups,
                Departments = departments
            };
            model.StudentCounts = new Dictionary<int, int>();
            foreach (var group in model.Groups)
            {
                int studentCount = await workBD.Students.CountAsync(s => s.GroupDep == group.Id);
                model.StudentCounts[group.Id] = studentCount;
            }
            return View(model);
        }
        public async Task<IActionResult> ClassroomPageView(int groupID)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var group = await workBD.Groups.FindAsync(groupID);
            var individualProjects = await workBD.IndividualProjects.ToListAsync();
            var students = await workBD.Students.Where(x => x.GroupDep == groupID && x.Role == 1).ToListAsync();
            if (group == null)
            {
                return RedirectToAction("ClassroomPage");
            }
            var model = new StudentsView();
            var debtorStatus = new Dictionary<int, bool>();
            foreach (var student in students)
            {
                bool isDebtor = true;
                var studentProjects = individualProjects.Where(p => p.Student == student.Id).ToList();

                if (studentProjects.Any())
                {
                    foreach (var project in studentProjects)
                    {
                        if (project.Gradle > 2)
                        {
                            isDebtor = false;
                            break;
                        }
                    }
                }

                debtorStatus[student.Id] = isDebtor;
            }
            model.IndividualProjects = individualProjects;
            model.nameGroup = group.Name;
            model.Students = students;
            model.DebtorStatus = debtorStatus;
            return View(model);
        }
        public async Task<IActionResult> StudentProjectsView(int studentId)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var individualProject = await workBD.IndividualProjects.Where(x => x.Student == studentId).ToListAsync();
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
        public async Task<IActionResult> AboutFeedback(int idProject)
        {
            var individualProject = await workBD.IndividualProjects.FindAsync(idProject);
            string gradle = "";
            string status = "";
            if (individualProject == null)
            {
                return RedirectToAction("StudentProjectsView");
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
            else if (individualProject.Status == 2)
            {
                status = "Отклонена";
            }
            else if (individualProject.Status == 3)
            {
                status = "Не просмотрено";
            }
            else if (individualProject.Status == 4)
            {
                status = "Оценено";
            }
            var model = new FeedbackViewModel();
            model.status = status;
            model.gradle = gradle;
            model.feedback = individualProject.Feedback;
            model.IndividualProject = individualProject;
            return View(model);
        }
        //public async Task<IActionResult> goBack(int idProject)
        //{
        //    var individualProject = await workBD.IndividualProjects.FindAsync(idProject);
        //    string gradle = "";
        //    string status = "";
        //    if (individualProject == null)
        //    {
        //        return RedirectToAction("StudentProjectsView");
        //    }
        //    if (individualProject.Gradle == null)
        //    {
        //        gradle = "Отсутствует";
        //    }
        //    else
        //    {
        //        gradle = individualProject.Gradle.ToString();
        //    }
        //    if (individualProject.Status == 1)
        //    {
        //        status = "Одобрено";
        //    }
        //    else if (individualProject.Status == 2)
        //    {
        //        status = "Отклонена";
        //    }
        //    else if (individualProject.Status == 3)
        //    {
        //        status = "Не просмотрено";
        //    }
        //    else if (individualProject.Status == 4)
        //    {
        //        status = "Оценено";
        //    }
        //    var model = new FeedbackViewModel();
        //    model.status = status;
        //    model.gradle = gradle;
        //    model.IndividualProject = individualProject;
        //    return View(model);
        //}
    }
}
