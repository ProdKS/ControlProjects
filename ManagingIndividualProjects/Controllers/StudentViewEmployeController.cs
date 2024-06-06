using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagingIndividualProjects.Controllers
{
    public class StudentViewEmployeController : Controller
    {
        private ProjectsContext workBD;
        public StudentViewEmployeController(ProjectsContext workbd)
        {
            this.workBD = workbd;

        }
        public async Task<IActionResult> StudentViewEmploye()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            int NowUserRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var students = await workBD.Students.Where(x => x.Role == 1).ToListAsync();
            var departments = new List<Department>();
            var groups = new List<Group>();

            foreach (var student in students)
            {
                if (student.GroupDep.HasValue)
                {
                    var groupStudent = await workBD.Groups.FirstOrDefaultAsync(d => d.Id == student.GroupDep.Value);
                    if (groupStudent != null)
                    {
                        groups.Add(groupStudent);
                        var department = await workBD.Departments.FirstOrDefaultAsync(d => d.Id == groupStudent.DepartmentId && groupStudent.IsDepartment > 6);
                        if (department != null)
                        {
                            departments.Add(department);
                        }
                    }
                }
            }
            if (NowUserRole == 4)
            {
                int DepartmentEmployee = Convert.ToInt32(HttpContext.Session.GetString("DepartmentEmployee"));
                var individualProjects = await workBD.IndividualProjects.ToListAsync();

                var studentsForEmployee = await workBD.Students
                    .Join(
                        workBD.Groups,
                        student => student.GroupDep,
                        group => group.Id,
                        (student, group) => new { student, group }
                    )
                    .Where(sg => sg.student.Role == 1 && sg.group.DepartmentId == DepartmentEmployee && sg.group.IsDepartment == 0)
                    .Select(sg => sg.student)
                    .ToListAsync();

                var debtorStatus = new Dictionary<int, bool>();
                foreach (var student in studentsForEmployee)
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
                    groups = groups,
                    Students = studentsForEmployee,
                    DebtorStatus = debtorStatus
                };
                return View(model);
            }

            return View();
        }
    }
}

