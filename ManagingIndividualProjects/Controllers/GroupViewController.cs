using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagingIndividualProjects.Controllers
{
    public class GroupViewController : Controller
    {
        private ProjectsContext workBD;
        public GroupViewController(ProjectsContext workbd)
        {
            this.workBD = workbd;
        }
        public IActionResult GroupView()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var groups = workBD.Groups.Where(x => x.IsDepartment == 0).ToList();            
            return View(groups);
        }
        public async Task<IActionResult> GroupDetailView(int groupsId)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var group = await workBD.Groups.FindAsync(groupsId);
            var individualProjects = await workBD.IndividualProjects.ToListAsync();
            if (group == null)
            {
                return RedirectToAction("GroupView");
            }
            var students = await workBD.Students.Where(x => x.GroupDep == groupsId && x.Role == 1).ToListAsync();
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

            var model = new StudentsView
            {
                IndividualProjects = individualProjects,
                nameGroup = group.Name,
                Students = students,
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
