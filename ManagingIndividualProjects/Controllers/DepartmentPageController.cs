using CsvHelper;
using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Formats.Asn1;
using System.Linq;
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
            int nowUserRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            DepartmentListViewModel viewModel = new DepartmentListViewModel
            {
                Departments = new List<Department>(),
                groupsCounts = new Dictionary<int, int>()
            };
            viewModel.Departments = await workBD.Departments.ToListAsync();
            foreach (var department in viewModel.Departments)
            {
                int groupCount = await workBD.Groups.CountAsync(g => g.DepartmentId == department.Id && g.IsDepartment == 0);
                viewModel.groupsCounts.Add(department.Id, groupCount);
            }
            return View(viewModel);
        }

        public async Task<IActionResult> DepartmentDetailView(int departmentId)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var department = await workBD.Departments.FindAsync(departmentId);
            var employees = await workBD.Employees.ToListAsync();
            if (department == null)
            {
                return RedirectToAction("DepartmentListView");
            }
            var model = new DepartmentDetail();
            model.NameDepartment = department.Name;
            model.Groups = await workBD.Groups.Where(x => x.DepartmentId == departmentId && x.IsDepartment == 0).ToListAsync();
            model.employees = employees;
            model.StudentCounts = new Dictionary<int, int>();
            foreach (var group in model.Groups)
            {
                int studentCount = await workBD.Students.CountAsync(s => s.GroupDep == group.Id);
                model.StudentCounts[group.Id] = studentCount;
            }

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
        public async Task<IActionResult> GenerateDocGroup(int groupid)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var group = await workBD.Groups.FindAsync(groupid);
            if (group == null)
            {
                return View();
            }
            var students = await workBD.Students
                .Where(s => s.GroupDep == groupid)
                .ToListAsync();
            var studentIds = students.Select(s => s.Id).ToList();

            var studentProjects = await workBD.IndividualProjects
                .Where(ip => studentIds.Contains(ip.Student.Value))
                .ToListAsync();
            var subjectIds = studentProjects.Select(ip => ip.Subject).ToList();
            var subjects = await workBD.Subjects
                .Where(s => subjectIds.Contains(s.Id))
                .ToListAsync();
            var teacherIds = subjects.Select(s => s.Teacherid).ToList();
            var teachers = await workBD.Employees
                .Where(e => teacherIds.Contains(e.Id))
                .ToListAsync();
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Students");

                worksheet.Cells[1, 1].Value = "Студент";
                worksheet.Cells[1, 2].Value = "Тема проекта";
                worksheet.Cells[1, 3].Value = "Оценка";
                worksheet.Cells[1, 4].Value = "Предмет";
                worksheet.Cells[1, 5].Value = "Преподаватель";
                worksheet.Cells[1, 6].Value = "Отзыв";
                int row = 2;
                foreach (var student in students)
                {
                    worksheet.Cells[row, 1].Value = $"{student.Surname} {student.Name} {student.Pat}";

                    var project = studentProjects.FirstOrDefault(ip => ip.Student == student.Id);
                    if (project != null)
                    {
                        worksheet.Cells[row, 2].Value = project.NameTheme ?? "отсутствует";
                        worksheet.Cells[row, 3].Value = project.Gradle?.ToString() ?? "отсутствует";

                        var subject = subjects.FirstOrDefault(s => s.Id == project.Subject);
                        if (subject != null)
                        {
                            worksheet.Cells[row, 4].Value = subject.Name ?? "отсутствует";

                            var teacher = teachers.FirstOrDefault(t => t.Id == subject.Teacherid);
                            if (teacher != null)
                            {
                                worksheet.Cells[row, 5].Value = $"{teacher.Surname} {teacher.Name} {teacher.Pat}";
                            }
                            else
                            {
                                worksheet.Cells[row, 5].Value = "отсутствует";
                            }
                        }
                        else
                        {
                            worksheet.Cells[row, 4].Value = "отсутствует";
                            worksheet.Cells[row, 5].Value = "отсутствует";
                        }
                        worksheet.Cells[row, 6].Value = project.Feedback;
                    }
                    else
                    {
                        worksheet.Cells[row, 2].Value = "отсутствует";
                        worksheet.Cells[row, 3].Value = "отсутствует";
                        worksheet.Cells[row, 4].Value = "отсутствует";
                        worksheet.Cells[row, 5].Value = "отсутствует";
                        worksheet.Cells[row, 6].Value = "отсутствует";
                    }
                    row++;
                }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                var fileName = $"Список_группы_{group.Name}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(stream, contentType, fileName);
            }
        }
        public async Task<IActionResult> GenerateDocDebtorsGroup(int groupid)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var group = await workBD.Groups.FindAsync(groupid);
            if (group == null)
            {
                return View();
            }
            var students = await workBD.Students
                .Where(s => s.GroupDep == groupid)
                .ToListAsync();
            var studentIds = students.Select(s => s.Id).ToList();

            var studentProjects = await workBD.IndividualProjects
                .Where(ip => studentIds.Contains(ip.Student.Value))
                .ToListAsync();

            var subjectIds = studentProjects.Select(ip => ip.Subject).ToList();

            var subjects = await workBD.Subjects
                .Where(s => subjectIds.Contains(s.Id))
                .ToListAsync();

            var teacherIds = subjects.Select(s => s.Teacherid).ToList();

            var teachers = await workBD.Employees
                .Where(e => teacherIds.Contains(e.Id))
                .ToListAsync();

            var debtors = students.Where(student =>
            {
                var project = studentProjects.FirstOrDefault(ip => ip.Student == student.Id);
                return project == null || project.Gradle == 2;
            }).ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Debtors");

                worksheet.Cells[1, 1].Value = "Студент";
                worksheet.Cells[1, 2].Value = "Тема проекта";
                worksheet.Cells[1, 3].Value = "Оценка";
                worksheet.Cells[1, 4].Value = "Предмет";
                worksheet.Cells[1, 5].Value = "Преподаватель";

                int row = 2;
                foreach (var student in debtors)
                {
                    worksheet.Cells[row, 1].Value = $"{student.Surname} {student.Name} {student.Pat}";

                    var project = studentProjects.FirstOrDefault(ip => ip.Student == student.Id);
                    if (project != null)
                    {
                        worksheet.Cells[row, 2].Value = project.NameTheme ?? "отсутствует";
                        worksheet.Cells[row, 3].Value = project.Gradle?.ToString() ?? "отсутствует";

                        var subject = subjects.FirstOrDefault(s => s.Id == project.Subject);
                        if (subject != null)
                        {
                            worksheet.Cells[row, 4].Value = subject.Name ?? "отсутствует";

                            var teacher = teachers.FirstOrDefault(t => t.Id == subject.Teacherid);
                            if (teacher != null)
                            {
                                worksheet.Cells[row, 5].Value = $"{teacher.Surname} {teacher.Name} {teacher.Pat}";
                            }
                            else
                            {
                                worksheet.Cells[row, 5].Value = "отсутствует";
                            }
                        }
                        else
                        {
                            worksheet.Cells[row, 4].Value = "отсутствует";
                            worksheet.Cells[row, 5].Value = "отсутствует";
                        }
                    }
                    else
                    {
                        worksheet.Cells[row, 2].Value = "отсутствует";
                        worksheet.Cells[row, 3].Value = "отсутствует";
                        worksheet.Cells[row, 4].Value = "отсутствует";
                        worksheet.Cells[row, 5].Value = "отсутствует";
                        var projectCell = worksheet.Cells[row, 2];
                    }
                    row++;
                }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                var fileName = $"Список_должников_группы_{group.Name}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(stream, contentType, fileName);
            }
        }
        public async Task<IActionResult> GenerateDocDepartment(int departmentId)
        {
            var department = await workBD.Departments.FindAsync(departmentId);
            if (department == null)
            {
                return View();
            }
            string departmentName = department.Name;
            var students = await workBD.Students
                .Where(s => workBD.Groups.Any(g => g.DepartmentId == departmentId && g.Id == s.GroupDep))
                .ToListAsync();
            var studentIds = students.Select(s => s.Id).ToList();
            var studentProjects = await workBD.IndividualProjects
                .Where(ip => studentIds.Contains(ip.Student.Value))
                .ToListAsync();
            var subjectIds = studentProjects.Select(ip => ip.Subject).ToList();
            var subjects = await workBD.Subjects
                .Where(s => subjectIds.Contains(s.Id))
                .ToListAsync();
            var teacherIds = subjects.Select(s => s.Teacherid).ToList();
            var teachers = await workBD.Employees
                .Where(e => teacherIds.Contains(e.Id))
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Students");

                worksheet.Cells[1, 1].Value = "Студент";
                worksheet.Cells[1, 2].Value = "Тема проекта";
                worksheet.Cells[1, 3].Value = "Оценка";
                worksheet.Cells[1, 4].Value = "Предмет";
                worksheet.Cells[1, 5].Value = "Преподаватель";
                worksheet.Cells[1, 6].Value = "Группа";

                int row = 2;
                foreach (var student in students)
                {
                    worksheet.Cells[row, 1].Value = $"{student.Surname} {student.Name} {student.Pat}";

                    var project = studentProjects.FirstOrDefault(ip => ip.Student == student.Id);
                    if (project != null)
                    {
                        worksheet.Cells[row, 2].Value = project.NameTheme ?? "отсутствует";
                        worksheet.Cells[row, 3].Value = project.Gradle?.ToString() ?? "отсутствует";

                        var subject = subjects.FirstOrDefault(s => s.Id == project.Subject);
                        if (subject != null)
                        {
                            worksheet.Cells[row, 4].Value = subject.Name ?? "отсутствует";

                            var teacher = teachers.FirstOrDefault(t => t.Id == subject.Teacherid);
                            if (teacher != null)
                            {
                                worksheet.Cells[row, 5].Value = $"{teacher.Surname} {teacher.Name} {teacher.Pat}";
                            }
                            else
                            {
                                worksheet.Cells[row, 5].Value = "отсутствует";
                            }
                        }
                        else
                        {
                            worksheet.Cells[row, 4].Value = "отсутствует";
                            worksheet.Cells[row, 5].Value = "отсутствует";
                        }
                    }
                    else
                    {
                        worksheet.Cells[row, 2].Value = "отсутствует";
                        worksheet.Cells[row, 3].Value = "отсутствует";
                        worksheet.Cells[row, 4].Value = "отсутствует";
                        worksheet.Cells[row, 5].Value = "отсутствует";

                        var projectCell = worksheet.Cells[row, 2];
                        projectCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        projectCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                        projectCell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    var group = await workBD.Groups.FirstOrDefaultAsync(g => g.Id == student.GroupDep);
                    worksheet.Cells[row, 6].Value = group?.Name ?? "отсутствует";
                    row++;
                }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                var fileName = $"Отделение_{departmentName}_отчет_по_студентам.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(stream, contentType, fileName);
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
            model.feedback = individualProject.Feedback;
            model.gradle = gradle;
            model.IndividualProject = individualProject;
            return View(model);
        }
        public async Task<IActionResult> GenerateDebtorsReportDepartment(int departmentId)
        {
            var department = await workBD.Departments.FindAsync(departmentId);
            if (department == null)
            {
                return View();
            }
            string departmentName = department.Name;
            var students = await workBD.Students
                .Where(s => workBD.Groups.Any(g => g.DepartmentId == departmentId && g.Id == s.GroupDep))
                .ToListAsync();
            var studentIds = students.Select(s => s.Id).ToList();
            var studentProjects = await workBD.IndividualProjects
                .Where(ip => studentIds.Contains(ip.Student.Value))
                .ToListAsync();
            var subjectIds = studentProjects.Select(ip => ip.Subject).ToList();
            var subjects = await workBD.Subjects
                .Where(s => subjectIds.Contains(s.Id))
                .ToListAsync();
            var teacherIds = subjects.Select(s => s.Teacherid).ToList();
            var teachers = await workBD.Employees
                .Where(e => teacherIds.Contains(e.Id))
                .ToListAsync();

            var debtors = students.Where(student =>
            {
                var project = studentProjects.FirstOrDefault(ip => ip.Student == student.Id);
                return project == null || project.Gradle == 2;
            }).ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Debtors");

                worksheet.Cells[1, 1].Value = "Студент";
                worksheet.Cells[1, 2].Value = "Тема проекта";
                worksheet.Cells[1, 3].Value = "Оценка";
                worksheet.Cells[1, 4].Value = "Предмет";
                worksheet.Cells[1, 5].Value = "Преподаватель";
                worksheet.Cells[1, 6].Value = "Группа";

                int row = 2;
                foreach (var student in debtors)
                {
                    worksheet.Cells[row, 1].Value = $"{student.Surname} {student.Name} {student.Pat}";

                    var project = studentProjects.FirstOrDefault(ip => ip.Student == student.Id);
                    if (project != null)
                    {
                        worksheet.Cells[row, 2].Value = project.NameTheme ?? "отсутствует";
                        worksheet.Cells[row, 3].Value = project.Gradle?.ToString() ?? "отсутствует";

                        var subject = subjects.FirstOrDefault(s => s.Id == project.Subject);
                        if (subject != null)
                        {
                            worksheet.Cells[row, 4].Value = subject.Name ?? "отсутствует";

                            var teacher = teachers.FirstOrDefault(t => t.Id == subject.Teacherid);
                            if (teacher != null)
                            {
                                worksheet.Cells[row, 5].Value = $"{teacher.Surname} {teacher.Name} {teacher.Pat}";
                            }
                            else
                            {
                                worksheet.Cells[row, 5].Value = "отсутствует";
                            }
                        }
                        else
                        {
                            worksheet.Cells[row, 4].Value = "отсутствует";
                            worksheet.Cells[row, 5].Value = "отсутствует";
                        }
                    }
                    else
                    {
                        worksheet.Cells[row, 2].Value = "отсутствует";
                        worksheet.Cells[row, 3].Value = "отсутствует";
                        worksheet.Cells[row, 4].Value = "отсутствует";
                        worksheet.Cells[row, 5].Value = "отсутствует";

                        var projectCell = worksheet.Cells[row, 2];
                    }

                    var group = await workBD.Groups.FirstOrDefaultAsync(g => g.Id == student.GroupDep);
                    worksheet.Cells[row, 6].Value = group?.Name ?? "отсутствует";
                    row++;
                }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                var fileName = $"Отделение_{departmentName}_отчет_по_студентам.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(stream, contentType, fileName);
            }
        }
    }

    // импорт файлов
    //public void ImportStudentsCsv(string filePath)
    //{
    //    using (var reader = new StreamReader(filePath))
    //    using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
    //    {
    //        var records = csv.GetRecords<StudentCsvModel>().ToList();

    //        foreach (var record in records)
    //        {
    //            var student = new Student
    //            {
    //                Login = record.Login,
    //                Password = record.Password,
    //                Surname = record.Surname,
    //                Name = record.Name,
    //                Pat = record.Pat,
    //                Number = record.Number,
    //                Role = record.Role,
    //                GroupDep = record.GroupDep
    //            };

    //            workBD.Students.Add(student);
    //        }

    //        workBD.SaveChanges();
    //    }
    //}
}
