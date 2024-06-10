using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ManagingIndividualProjects.Controllers
{
    public class StudentViewController : Controller
    {
        private ProjectsContext workBD;
        public StudentViewController(ProjectsContext workbd)
        {
            this.workBD = workbd;

        }
        public async Task<IActionResult> StudentView()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var students = await workBD.Students.Where(x => x.Role == 1).ToListAsync();
            var departments = new List<Department>();
            var individualProjects = await workBD.IndividualProjects.ToListAsync();
            var groups = new List<Models.Group>();
            var debtorStatus = new Dictionary<int, bool>(); 
            foreach (var student in students)
            {
                bool isDebtor = true;
                if (student.GroupDep.HasValue)
                {
                    var groupStudent = await workBD.Groups.FirstOrDefaultAsync(d => d.Id == student.GroupDep.Value);
                    if (groupStudent != null)
                    {
                        groups.Add(groupStudent);
                        var department = await workBD.Departments.FirstOrDefaultAsync(d => d.Id == groupStudent.DepartmentId.Value && groupStudent.IsDepartment > 6);
                        if (department != null)
                        {
                            departments.Add(department);
                        }
                    }
                }
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

            var model = new StudentViewModel
            {
                individualProjects = individualProjects,
                Students = students,
                Groups = groups,
                Departments = departments,
                DebtorStatus = debtorStatus
            };
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
    }
}
