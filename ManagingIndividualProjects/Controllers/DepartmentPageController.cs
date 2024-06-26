using CsvHelper;
using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Formats.Asn1;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf.AcroForms;


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
        public string GradleToText(int? gradle)
        {
            string gradleText = "";
            if (gradle != null)
            {
                if (gradle == 5)
                {
                    gradleText = "Отлично";
                }
                else if (gradle == 4)
                {
                    gradleText = "Хорошо";
                }
                else if (gradle == 3)
                {
                    gradleText = "Удовл";
                }
                else if (gradle == 2)
                {
                    gradleText = "Неуд.";
                }
                else gradleText = "";
            }
            return gradleText;
        }
        
        public async Task<IActionResult> GenerateDocGroup(int groupid)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var group = await workBD.Groups.FindAsync(groupid);
            if (group == null)
            {
                return View();
            }
            var departments = await workBD.Departments.Where(x => x.Id == group.DepartmentId).ToListAsync();
            string departmentName = departments.FirstOrDefault()?.Name;
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
            var studentReportModels = students.Select((student, index) =>
            {
                var project = studentProjects.FirstOrDefault(ip => ip.Student == student.Id);
                var subject = project != null ? subjects.FirstOrDefault(s => s.Id == project.Subject) : null;
                string gradletext = "";
                return new StudentReportModel
                {
                    Number = index + 1,
                    FullName = $"{student.Surname} {student.Name} {student.Pat}",
                    Subject = subject?.Name ?? "",
                    Grade = project?.Gradle?.ToString() ?? "",
                    GradeText = GradleToText(project?.Gradle)
                };
            }).ToList();
            using (var stream = new MemoryStream())
            {
                var pdf = new PdfDocument();
                var page = pdf.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Arial", 10);
                var boldFont = new XFont("Arial", 10);
                var margin = 40; 
                var rect = new XRect(margin, margin, page.Width - 2 * margin, page.Height - 2 * margin);
                var rect2 = new XRect(margin, 60, page.Width - 2 * margin, page.Height - 2 * margin);
                gfx.DrawString("Краевое государственное автономное профессиональное образовательное учреждение \"Пермский\"", boldFont, XBrushes.Black, rect, XStringFormats.TopCenter);
                gfx.DrawString("авиационный техникум им.А.Д.Швецова\"", boldFont, XBrushes.Black, rect2, XStringFormats.TopCenter);
                gfx.DrawString("Семестр: 2       20   - 20    учебного года", font, XBrushes.Black, new XRect(rect.Left, rect.Top + 40, rect.Width, 20), XStringFormats.TopLeft);
                gfx.DrawString("№                    ", font, XBrushes.Black, new XRect(rect.Right - 100, rect.Top + 40, 60, 20), XStringFormats.TopRight);
                gfx.DrawString("Форма контроля: Индивидуальный проект", font, XBrushes.Black, new XRect(rect.Left, rect.Top + 60, rect.Width, 20), XStringFormats.TopLeft);
                gfx.DrawString("Дата проведения         ", font, XBrushes.Black, new XRect(rect.Right - 200, rect.Top + 60, 200, 20), XStringFormats.TopRight);
                gfx.DrawString($"Отделение: {departmentName ?? "отсутствует"}", font, XBrushes.Black, new XRect(rect.Left, rect.Top + 80, rect.Width, 20), XStringFormats.TopLeft);
                gfx.DrawString($"Группа: {group.Name}        ", font, XBrushes.Black, new XRect(rect.Right - 200, rect.Top + 80, 200, 20), XStringFormats.TopRight);
                gfx.DrawString("Специальность: 09.02.07 Информационные системы и программирование", font, XBrushes.Black, new XRect(rect.Left, rect.Top + 100, rect.Width, 20), XStringFormats.TopLeft);
                var tableStartY = rect.Top + 130;
                var tableCellHeight = 20;
                var tableColumnWidths = new[] { 30, 180, 140, 50, 90, 60 };
                gfx.DrawRectangle(XPens.Black, rect.Left, tableStartY, tableColumnWidths.Sum(), tableCellHeight * 2);
                gfx.DrawRectangle(XPens.Black, rect.Left, tableStartY, tableColumnWidths[0], tableCellHeight * 2);
                gfx.DrawRectangle(XPens.Black, rect.Left + tableColumnWidths[0], tableStartY, tableColumnWidths[1], tableCellHeight * 2);
                gfx.DrawRectangle(XPens.Black, rect.Left + tableColumnWidths[0] + tableColumnWidths[1], tableStartY, tableColumnWidths[2], tableCellHeight);
                gfx.DrawRectangle(XPens.Black, rect.Left + tableColumnWidths[0] + tableColumnWidths[1] + tableColumnWidths[2], tableStartY, tableColumnWidths[3] + tableColumnWidths[4], tableCellHeight);
                gfx.DrawRectangle(XPens.Black, rect.Left + tableColumnWidths[0] + tableColumnWidths[1] + tableColumnWidths[2], tableStartY + tableCellHeight, tableColumnWidths[3], tableCellHeight);
                gfx.DrawRectangle(XPens.Black, rect.Left + tableColumnWidths[0] + tableColumnWidths[1] + tableColumnWidths[2] + tableColumnWidths[3], tableStartY + tableCellHeight, tableColumnWidths[4], tableCellHeight);
                gfx.DrawRectangle(XPens.Black, rect.Left + tableColumnWidths[0] + tableColumnWidths[1] + tableColumnWidths[2] + tableColumnWidths[3] + tableColumnWidths[4], tableStartY, tableColumnWidths[5], tableCellHeight * 2);
                gfx.DrawString("№", boldFont, XBrushes.Black, new XRect(rect.Left, tableStartY, tableColumnWidths[0], tableCellHeight * 2), XStringFormats.Center);
                gfx.DrawString("Фамилия, Имя, Отчество", boldFont, XBrushes.Black, new XRect(rect.Left + tableColumnWidths[0], tableStartY, tableColumnWidths[1], tableCellHeight * 2), XStringFormats.Center);
                gfx.DrawString("Дисциплина", boldFont, XBrushes.Black, new XRect(rect.Left + tableColumnWidths[0] + tableColumnWidths[1], tableStartY, tableColumnWidths[2], tableCellHeight), XStringFormats.Center);
                gfx.DrawString("Дифференцированный зачет", boldFont, XBrushes.Black, new XRect(rect.Left + tableColumnWidths[0] + tableColumnWidths[1] + tableColumnWidths[2], tableStartY, tableColumnWidths[3] + tableColumnWidths[4], tableCellHeight), XStringFormats.Center);
                gfx.DrawString("Цифрой", boldFont, XBrushes.Black, new XRect(rect.Left + tableColumnWidths[0] + tableColumnWidths[1] + tableColumnWidths[2], tableStartY + tableCellHeight, tableColumnWidths[3], tableCellHeight), XStringFormats.Center);
                gfx.DrawString("Полностью", boldFont, XBrushes.Black, new XRect(rect.Left + tableColumnWidths[0] + tableColumnWidths[1] + tableColumnWidths[2] + tableColumnWidths[3], tableStartY + tableCellHeight, tableColumnWidths[4], tableCellHeight), XStringFormats.Center);
                gfx.DrawString("Подпись", boldFont, XBrushes.Black, new XRect(rect.Left + tableColumnWidths[0] + tableColumnWidths[1] + tableColumnWidths[2] + tableColumnWidths[3] + tableColumnWidths[4], tableStartY, tableColumnWidths[5], tableCellHeight * 2), XStringFormats.Center);
                var currentY = tableStartY + tableCellHeight * 2;
                foreach (var student in studentReportModels)
                {
                    gfx.DrawRectangle(XPens.Black, rect.Left, currentY, tableColumnWidths[0], tableCellHeight);
                    gfx.DrawRectangle(XPens.Black, rect.Left + tableColumnWidths[0], currentY, tableColumnWidths[1], tableCellHeight);
                    gfx.DrawRectangle(XPens.Black, rect.Left + tableColumnWidths[0] + tableColumnWidths[1], currentY, tableColumnWidths[2], tableCellHeight);
                    gfx.DrawRectangle(XPens.Black, rect.Left + tableColumnWidths[0] + tableColumnWidths[1] + tableColumnWidths[2], currentY, tableColumnWidths[3], tableCellHeight);
                    gfx.DrawRectangle(XPens.Black, rect.Left + tableColumnWidths[0] + tableColumnWidths[1] + tableColumnWidths[2] + tableColumnWidths[3], currentY, tableColumnWidths[4], tableCellHeight);
                    gfx.DrawRectangle(XPens.Black, rect.Left + tableColumnWidths[0] + tableColumnWidths[1] + tableColumnWidths[2] + tableColumnWidths[3] + tableColumnWidths[4], currentY, tableColumnWidths[5], tableCellHeight);

                    gfx.DrawString(student.Number.ToString(), font, XBrushes.Black, new XRect(rect.Left, currentY, tableColumnWidths[0], tableCellHeight), XStringFormats.Center);
                    gfx.DrawString(student.FullName, font, XBrushes.Black, new XRect(rect.Left + tableColumnWidths[0], currentY, tableColumnWidths[1], tableCellHeight), XStringFormats.Center);
                    gfx.DrawString(student.Subject, font, XBrushes.Black, new XRect(rect.Left + tableColumnWidths[0] + tableColumnWidths[1], currentY, tableColumnWidths[2], tableCellHeight), XStringFormats.Center);
                    gfx.DrawString(student.Grade, font, XBrushes.Black, new XRect(rect.Left + tableColumnWidths[0] + tableColumnWidths[1] + tableColumnWidths[2], currentY, tableColumnWidths[3], tableCellHeight), XStringFormats.Center);
                    gfx.DrawString(student.GradeText, font, XBrushes.Black, new XRect(rect.Left + tableColumnWidths[0] + tableColumnWidths[1] + tableColumnWidths[2] + tableColumnWidths[3], currentY, tableColumnWidths[4], tableCellHeight), XStringFormats.Center);
                    gfx.DrawString(string.Empty, font, XBrushes.Black, new XRect(rect.Left + tableColumnWidths[0] + tableColumnWidths[1] + tableColumnWidths[2] + tableColumnWidths[3] + tableColumnWidths[4], currentY, tableColumnWidths[5], tableCellHeight), XStringFormats.Center);

                    currentY += tableCellHeight;
                }

                pdf.Save(stream);
                var fileName = $"Сводка_{group.Name}_Индивидуальные_проекты.pdf";
                return File(stream.ToArray(), "application/pdf", fileName);
            }
        }
        private PdfDocument CreatePdfDocument(List<StudentReportModel> students, string departmentName, string groupName)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 10);
            var boldFont = new XFont("Arial", 10);
            double marginLeft = 40;
            double marginTop = 60;
            double tableWidth = page.Width - 2 * marginLeft;
            double col1Width = 30;
            double col2Width = 150;
            double col3Width = 100;
            double col4Width = 60;
            double col5Width = 60;
            double rowHeight = 20;
            gfx.DrawString("Краевое государственное автономное профессиональное образовательное учреждение \"Пермский авиационный техникум им. А.Д.Швецова\"", boldFont, XBrushes.Black, new XRect(0, 20, page.Width, 30), XStringFormats.TopCenter);
            gfx.DrawString($"Отделение: {departmentName}", font, XBrushes.Black, new XRect(marginLeft, marginTop, tableWidth, rowHeight), XStringFormats.TopLeft);
            gfx.DrawString($"Группа: {groupName}", font, XBrushes.Black, new XRect(marginLeft, marginTop + rowHeight, tableWidth, rowHeight), XStringFormats.TopLeft);
            double currentY = marginTop + 2 * rowHeight;
            gfx.DrawRectangle(XPens.Black, marginLeft, currentY, col1Width, rowHeight);
            gfx.DrawRectangle(XPens.Black, marginLeft + col1Width, currentY, col2Width, rowHeight);
            gfx.DrawRectangle(XPens.Black, marginLeft + col1Width + col2Width, currentY, col3Width, rowHeight);
            gfx.DrawRectangle(XPens.Black, marginLeft + col1Width + col2Width + col3Width, currentY, col4Width, rowHeight);
            gfx.DrawRectangle(XPens.Black, marginLeft + col1Width + col2Width + col3Width + col4Width, currentY, col5Width, rowHeight);

            gfx.DrawString("№", boldFont, XBrushes.Black, new XRect(marginLeft, currentY, col1Width, rowHeight), XStringFormats.Center);
            gfx.DrawString("Фамилия, Имя, Отчество", boldFont, XBrushes.Black, new XRect(marginLeft + col1Width, currentY, col2Width, rowHeight), XStringFormats.Center);
            gfx.DrawString("Дисциплина", boldFont, XBrushes.Black, new XRect(marginLeft + col1Width + col2Width, currentY, col3Width, rowHeight), XStringFormats.Center);
            gfx.DrawString("Цифрой", boldFont, XBrushes.Black, new XRect(marginLeft + col1Width + col2Width + col3Width, currentY, col4Width, rowHeight), XStringFormats.Center);
            gfx.DrawString("Полностью", boldFont, XBrushes.Black, new XRect(marginLeft + col1Width + col2Width + col3Width + col4Width, currentY, col5Width, rowHeight), XStringFormats.Center);

            // Table rows
            currentY += rowHeight;
            int studentIndex = 1;
            foreach (var student in students)
            {
                gfx.DrawRectangle(XPens.Black, marginLeft, currentY, col1Width, rowHeight);
                gfx.DrawRectangle(XPens.Black, marginLeft + col1Width, currentY, col2Width, rowHeight);
                gfx.DrawRectangle(XPens.Black, marginLeft + col1Width + col2Width, currentY, col3Width, rowHeight);
                gfx.DrawRectangle(XPens.Black, marginLeft + col1Width + col2Width + col3Width, currentY, col4Width, rowHeight);
                gfx.DrawRectangle(XPens.Black, marginLeft + col1Width + col2Width + col3Width + col4Width, currentY, col5Width, rowHeight);

                gfx.DrawString(studentIndex.ToString(), font, XBrushes.Black, new XRect(marginLeft, currentY, col1Width, rowHeight), XStringFormats.Center);
                gfx.DrawString(TrimTextToFit(student.FullName, font, col2Width, gfx), font, XBrushes.Black, new XRect(marginLeft + col1Width, currentY, col2Width, rowHeight), XStringFormats.Center);
                gfx.DrawString("История", font, XBrushes.Black, new XRect(marginLeft + col1Width + col2Width, currentY, col3Width, rowHeight), XStringFormats.Center);

                currentY += rowHeight;
                studentIndex++;
            }

            // Footer
            currentY += rowHeight;
            gfx.DrawString("Зав отделением: Куртагина М. В.", boldFont, XBrushes.Black, new XRect(marginLeft, currentY, tableWidth, rowHeight), XStringFormats.TopLeft);

            return document;
        }

        private string TrimTextToFit(string text, XFont font, double width, XGraphics gfx)
        {
            while (gfx.MeasureString(text, font).Width > width)
            {
                if (text.Length > 3)
                {
                    text = text.Substring(0, text.Length - 4) + "...";
                }
                else
                {
                    return "...";
                }
            }
            return text;
        }

        public class StudentReportModel
        {
            public int Number { get; set; }
            public string FullName { get; set; }
            public string Subject { get; set; }
            public string Grade { get; set; }
            public string GradeText { get; set; }
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
    //public async Task<IActionResult> GenerateDocGroup(int groupid)
    //{
    //    ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
    //    var group = await workBD.Groups.FindAsync(groupid);
    //    if (group == null)
    //    {
    //        return View();
    //    }
    //    var students = await workBD.Students
    //        .Where(s => s.GroupDep == groupid)
    //        .ToListAsync();
    //    var studentIds = students.Select(s => s.Id).ToList();

    //    var studentProjects = await workBD.IndividualProjects
    //        .Where(ip => studentIds.Contains(ip.Student.Value))
    //        .ToListAsync();
    //    var subjectIds = studentProjects.Select(ip => ip.Subject).ToList();
    //    var subjects = await workBD.Subjects
    //        .Where(s => subjectIds.Contains(s.Id))
    //        .ToListAsync();
    //    var teacherIds = subjects.Select(s => s.Teacherid).ToList();
    //    var teachers = await workBD.Employees
    //        .Where(e => teacherIds.Contains(e.Id))
    //        .ToListAsync();
    //    using (var package = new ExcelPackage())
    //    {
    //        var worksheet = package.Workbook.Worksheets.Add("Students");

    //        worksheet.Cells[1, 1].Value = "Студент";
    //        worksheet.Cells[1, 2].Value = "Тема проекта";
    //        worksheet.Cells[1, 3].Value = "Оценка";
    //        worksheet.Cells[1, 4].Value = "Предмет";
    //        worksheet.Cells[1, 5].Value = "Преподаватель";
    //        worksheet.Cells[1, 6].Value = "Отзыв";
    //        int row = 2;
    //        foreach (var student in students)
    //        {
    //            worksheet.Cells[row, 1].Value = $"{student.Surname} {student.Name} {student.Pat}";

    //            var project = studentProjects.FirstOrDefault(ip => ip.Student == student.Id);
    //            if (project != null)
    //            {
    //                worksheet.Cells[row, 2].Value = project.NameTheme ?? "отсутствует";
    //                worksheet.Cells[row, 3].Value = project.Gradle?.ToString() ?? "отсутствует";

    //                var subject = subjects.FirstOrDefault(s => s.Id == project.Subject);
    //                if (subject != null)
    //                {
    //                    worksheet.Cells[row, 4].Value = subject.Name ?? "отсутствует";

    //                    var teacher = teachers.FirstOrDefault(t => t.Id == subject.Teacherid);
    //                    if (teacher != null)
    //                    {
    //                        worksheet.Cells[row, 5].Value = $"{teacher.Surname} {teacher.Name} {teacher.Pat}";
    //                    }
    //                    else
    //                    {
    //                        worksheet.Cells[row, 5].Value = "отсутствует";
    //                    }
    //                }
    //                else
    //                {
    //                    worksheet.Cells[row, 4].Value = "отсутствует";
    //                    worksheet.Cells[row, 5].Value = "отсутствует";
    //                }
    //                worksheet.Cells[row, 6].Value = project.Feedback;
    //            }
    //            else
    //            {
    //                worksheet.Cells[row, 2].Value = "отсутствует";
    //                worksheet.Cells[row, 3].Value = "отсутствует";
    //                worksheet.Cells[row, 4].Value = "отсутствует";
    //                worksheet.Cells[row, 5].Value = "отсутствует";
    //                worksheet.Cells[row, 6].Value = "отсутствует";
    //            }
    //            row++;
    //        }
    //        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
    //        var stream = new MemoryStream();
    //        package.SaveAs(stream);
    //        stream.Position = 0;
    //        var fileName = $"Список_группы_{group.Name}.xlsx";
    //        var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    //        return File(stream, contentType, fileName);
    //    }
    //}
}
