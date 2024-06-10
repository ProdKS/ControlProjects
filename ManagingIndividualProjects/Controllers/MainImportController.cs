using CsvHelper.Configuration;
using CsvHelper;
using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ManagingIndividualProjects.Controllers
{
    public class MainImportController : Controller
    {
        private ProjectsContext workBD;
        public MainImportController(ProjectsContext workbd)
        {
            this.workBD = workbd;
        }
        public IActionResult MainImport()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            return View();
        }
        public IActionResult ImportTeachers()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            return View();
        }
        public IActionResult ImportSubjects()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            return View();
        }
        public async Task<IActionResult> ImportGroupsAsync()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var departments = await workBD.Departments.ToListAsync();
            ViewBag.Departments = departments;
            return View();
        }
        public IActionResult ImportGroupsEmployee()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            return View();
        }
        public IActionResult ImportClassrooms()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            return View();
        }
        public async Task<IActionResult> ImportStudents()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var groups = await workBD.Groups.Where(x => x.IsDepartment != 1).ToListAsync();
            ViewBag.Groups = groups;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ImportSubjects(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please upload a valid file.");
                return RedirectToAction("ImportSubjects");
            }
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    MissingFieldFound = null
                };
                using (var csv = new CsvReader(reader, config))
                {
                    var subjects = new List<Subject>();
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        try
                        {
                            var subjectName = csv.GetField<string>(0);
                            var teacherFullName = csv.GetField<string>(1).Split(' ');
                            var surname = teacherFullName[0];
                            var name = teacherFullName[1];
                            var pat = teacherFullName.Length > 2 ? teacherFullName[2] : string.Empty;
                            var teacher = await workBD.Employees.FirstOrDefaultAsync(x => x.Surname == surname && x.Name == name && x.Pat == pat);
                            if (teacher != null)
                            {
                                subjects.Add(new Subject
                                {
                                    Name = subjectName,
                                    Teacherid = teacher.Id
                                });
                            }
                            else
                            {
                                ModelState.AddModelError("File", $"Учитель с ФИО \"{surname} {name} {pat}\" не найден.");
                                return RedirectToAction("ImportSubjects");
                            }
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("File", $"Ошибка при обработке строки: {csv.Context}. Ошибка: {ex.Message}");
                            return RedirectToAction("ImportSubjects");
                        }
                    }

                    await workBD.Subjects.AddRangeAsync(subjects);
                    await workBD.SaveChangesAsync();
                }
            }
            return RedirectToAction("MainImport");
        }
        [HttpPost]
        public async Task<IActionResult> ImportStudents(IFormFile file, int selectedGroupId)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please upload a valid file.");
                return RedirectToAction("ImportStudents");
            }

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    MissingFieldFound = null
                };
                using (var csv = new CsvReader(reader, config))
                {
                    var students = new List<Student>();
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        try
                        {
                            var surname = csv.GetField<string>(0);
                            var namePat = csv.GetField<string>(1).Split(' ');
                            var name = namePat[0];
                            var pat = namePat.Length > 1 ? namePat[1] : string.Empty;
                            var login = csv.GetField<string>(2);
                            var password = csv.GetField<string>(3);
                            var phone = csv.GetField<string>(5);

                            students.Add(new Student
                            {
                                Surname = surname,
                                Name = name,
                                Pat = pat,
                                Login = login,
                                Password = password,
                                Number = phone,
                                Role = 1,
                                GroupDep = selectedGroupId
                            });
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("File", $"Ошибка при обработке строки: {csv.Context}. Ошибка: {ex.Message}");
                            return RedirectToAction("ImportStudents");
                        }
                    }

                    await workBD.Students.AddRangeAsync(students);
                    await workBD.SaveChangesAsync();
                }
            }
            return RedirectToAction("MainImport");
        }
        [HttpPost]
        public async Task<IActionResult> ImportTeachers(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please upload a valid file.");
                return RedirectToAction("ImportTeachers");
            }

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    MissingFieldFound = null
                };
                using (var csv = new CsvReader(reader, config))
                {
                    var teachers = new List<Employee>();
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        try
                        {
                            var surname = csv.GetField<string>(0);
                            var namePat = csv.GetField<string>(1).Split(' ');
                            var name = namePat[0];
                            var pat = namePat.Length > 1 ? namePat[1] : string.Empty;
                            var login = csv.GetField<string>(2);
                            var password = csv.GetField<string>(3);
                            var phone = csv.GetField<string>(5);

                            teachers.Add(new Employee
                            {
                                Surname = surname,
                                Name = name,
                                Pat = pat,
                                Login = login,
                                Password = password,
                                Number = phone,
                                Role = 2
                            });
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("File", $"Ошибка при обработке строки: {csv.Context}. Ошибка: {ex.Message}");
                            return RedirectToAction("ImportTeachers");
                        }
                    }

                    await workBD.Employees.AddRangeAsync(teachers);
                    await workBD.SaveChangesAsync();
                }
            }
            return RedirectToAction("MainImport");
        }
        [HttpPost]
        public async Task<IActionResult> ImportClassrooms(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please upload a valid file.");
                return RedirectToAction("ImportClassroomTeachers");
            }

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    MissingFieldFound = null
                };
                using (var csv = new CsvReader(reader, config))
                {
                    var classroomTeachers = new List<Employee>();
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        try
                        {
                            var surname = csv.GetField<string>(0);
                            var namePat = csv.GetField<string>(1).Split(' ');
                            var name = namePat[0];
                            var pat = namePat.Length > 1 ? namePat[1] : string.Empty;
                            var login = csv.GetField<string>(2);
                            var password = csv.GetField<string>(3);
                            var phone = csv.GetField<string>(5);

                            classroomTeachers.Add(new Employee
                            {
                                Surname = surname,
                                Name = name,
                                Pat = pat,
                                Login = login,
                                Password = password,
                                Number = phone,
                                Role = 3
                            });
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("File", $"Ошибка при обработке строки: {csv.Context}. Ошибка: {ex.Message}");
                            return RedirectToAction("ImportClassroomTeachers");
                        }
                    }

                    await workBD.Employees.AddRangeAsync(classroomTeachers);
                    await workBD.SaveChangesAsync();
                }
            }
            return RedirectToAction("MainImport");
        }
        [HttpPost]
        public async Task<IActionResult> ImportGroups(IFormFile file, int selectedDepartmentId)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please upload a valid file.");
                return RedirectToAction("ImportGroups");
            }

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    MissingFieldFound = null
                };
                using (var csv = new CsvReader(reader, config))
                {
                    var groups = new List<Group>();
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        try
                        {
                            var groupName = csv.GetField<string>(0);
                            var teacherFio = csv.GetField<string>(1).Split(' ');
                            var teacherSurname = teacherFio[0];
                            var teacherName = teacherFio.Length > 1 ? teacherFio[1] : string.Empty;
                            var teacherPat = teacherFio.Length > 2 ? teacherFio[2] : string.Empty;
                            var teacher = await workBD.Employees
                                .FirstOrDefaultAsync(e => e.Surname == teacherSurname && e.Name == teacherName && e.Pat == teacherPat);

                            if (teacher == null)
                            {
                                ModelState.AddModelError("File", $"Teacher not found: {teacherSurname} {teacherName} {teacherPat}");
                                return RedirectToAction("ImportGroups");
                            }

                            groups.Add(new Group
                            {
                                Name = groupName,
                                ClassroomTeacher = teacher.Id,
                                DepartmentId = selectedDepartmentId
                            });
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("File", $"Error processing line: {csv.Context}. Error: {ex.Message}");
                            return RedirectToAction("ImportGroups");
                        }
                    }

                    await workBD.Groups.AddRangeAsync(groups);
                    await workBD.SaveChangesAsync();
                }
            }
            return RedirectToAction("MainImport");
        }
        [HttpPost]
        public async Task<IActionResult> ImportEmployeeGroups(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please upload a valid file.");
                return RedirectToAction("ImportEmployeeGroups");
            }

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    MissingFieldFound = null
                };
                using (var csv = new CsvReader(reader, config))
                {
                    var employeeGroups = new List<EmployeeGroup>();
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        try
                        {
                            var groupName = csv.GetField<string>(0);
                            var teacherFio = csv.GetField<string>(1).Split(' ');
                            var teacherSurname = teacherFio[0];
                            var teacherName = teacherFio.Length > 1 ? teacherFio[1] : string.Empty;
                            var teacherPat = teacherFio.Length > 2 ? teacherFio[2] : string.Empty;

                            var group = await workBD.Groups.FirstOrDefaultAsync(g => g.Name == groupName);
                            var teacher = await workBD.Employees.FirstOrDefaultAsync(e => e.Surname == teacherSurname && e.Name == teacherName && e.Pat == teacherPat);

                            if (group == null || teacher == null)
                            {
                                ModelState.AddModelError("File", $"Group or Teacher not found: {groupName}, {teacherSurname} {teacherName} {teacherPat}");
                                return RedirectToAction("ImportEmployeeGroups");
                            }

                            employeeGroups.Add(new EmployeeGroup
                            {
                                GroupId = group.Id,
                                TeacherId = teacher.Id
                            });
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("File", $"Error processing line: {csv.Context}. Error: {ex.Message}");
                            return RedirectToAction("ImportEmployeeGroups");
                        }
                    }

                    await workBD.EmployeeGroups.AddRangeAsync(employeeGroups);
                    await workBD.SaveChangesAsync();
                }
            }
            return RedirectToAction("MainImport");
        }
    }
}

