using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ManagingIndividualProjects.Controllers
{
    public class DepartmentPageController : Controller
    {
        private ProjectsContext workBD;
        public DepartmentPageController(ProjectsContext workbd)
        {
            this.workBD = workbd;
        }
        public async Task<IActionResult> DepartmentListViewAsync()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            int NowUserRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            if (NowUserRole == 5)
            {
                var departments = workBD.Departments.ToList();
                return View(departments);
            }
            else if (NowUserRole == 4)
            {
                int DepartmentEmployee = Convert.ToInt32(HttpContext.Session.GetString("DepartmentEmployee"));
                var departments = await workBD.Departments.Where(x => x.Id == DepartmentEmployee).ToListAsync();
                return View(departments);
            }
            return View();
        }
        public async Task<IActionResult> DepartmentDetailView(int departmentId)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var departments = await workBD.Departments.FindAsync(departmentId);
            if (departments == null)
            {
                return RedirectToAction("DepartmentListView");
            }
            var model = new DepartmentDetail();
            model.NameDepartment = departments.Name;
            model.Groups = await workBD.Groups.Where(x => x.DepartmentId == departmentId && x.IsDepartment == 0).ToListAsync();
            return View(model);
        }
        public async Task<IActionResult> GroupDetailView(int groupid)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var group = await workBD.Groups.FindAsync(groupid);
            var individualProjects = await workBD.IndividualProjects.ToListAsync();
            if (group == null)
            {
                return RedirectToAction("DepartmentListView");
            }
            var students = await workBD.Students.Where(x => x.GroupDep == groupid && x.Role == 1).ToListAsync();
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
