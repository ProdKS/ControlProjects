using CsvHelper.Configuration;
using CsvHelper;
using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            return RedirectToAction("ImportSubjects");
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
            return RedirectToAction("ImportStudents");
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
            return RedirectToAction("ImportTeachers");
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
            return RedirectToAction("ImportClassroomTeachers");
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
            return RedirectToAction("ImportGroups");
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
            return RedirectToAction("ImportEmployeeGroups");
        }
        public async Task<IActionResult> goToMain()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            return RedirectToAction("MainImport");
        }
        public async Task<IActionResult> goToTeachers()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            return RedirectToAction("ImportTeachers");
        }
        public async Task<IActionResult> goToSubjects()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            return RedirectToAction("ImportSubjects");
        }
        public async Task<IActionResult> goToGroups()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            return RedirectToAction("ImportGroups");
        }
        public async Task<IActionResult> goToEmployeeGroup()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            return RedirectToAction("ImportGroupsEmployee");
        }
        public IActionResult AddTeacher()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddTeacher(Employee teacher)
        {
            if (ModelState.IsValid)
            {
                if(teacher.Name != null)
                {
                    if(teacher.Surname != null)
                    {
                        if(teacher.Pat != null)
                        {
                            if(teacher.Login != null)
                            {
                                if(teacher.Password != null)
                                {
                                    if(teacher.Number != null)
                                    {
                                        teacher.Role = 3;
                                        workBD.Employees.Add(teacher);
                                        await workBD.SaveChangesAsync();
                                        return RedirectToAction("MainImport");
                                    }    
                                }
                            }
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("Ошибка", "Заполните данные");
                    return View();
                }
            }
            return View(teacher);
        }
        public IActionResult AddClassroom()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddClassroom(Employee teacher)
        {
            if (ModelState.IsValid)
            {
                if (teacher.Name != null)
                {
                    if (teacher.Surname != null)
                    {
                        if (teacher.Pat != null)
                        {
                            if (teacher.Login != null)
                            {
                                if (teacher.Password != null)
                                {
                                    if (teacher.Number != null)
                                    {
                                        teacher.Role = 2;
                                        workBD.Employees.Add(teacher);
                                        await workBD.SaveChangesAsync();
                                        return RedirectToAction("MainImport");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("Ошибка", "Заполните данные");
                    return View();
                }
            }
            return View(teacher);
        }
        public async Task<IActionResult> AppointClassroom()
        {
            var groups = await workBD.Groups
                .Where(g => g.ClassroomTeacher == null && g.IsDepartment != 1)
                .ToListAsync();
            ViewBag.Groups = groups;

            var teachers = await workBD.Employees
                .Where(e => e.Role == 2 || e.Role == 3)
                .ToListAsync();
            ViewBag.Teachers = teachers;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AppointClassroom(int selectedGroupId, int selectedClassroomId)
        {
            var group = await workBD.Groups.FindAsync(selectedGroupId);
            if (group == null)
            {
                ModelState.AddModelError("Group", "Группа не найдена");
                return RedirectToAction("AppointClassroom");
            }

            var teacher = await workBD.Employees.FindAsync(selectedClassroomId);
            if (teacher == null)
            {
                ModelState.AddModelError("Teacher", "Учитель не найден");
                return RedirectToAction("AppointClassroom");
            }

            group.ClassroomTeacher = selectedClassroomId;
            await workBD.SaveChangesAsync();

            return RedirectToAction("MainImport");
        }
        public async Task<IActionResult> AddSubject()
        {
            var teachers = await workBD.Employees
                .Where(e => e.Role == 3)
                .ToListAsync();
            ViewBag.Teachers = teachers;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddSubject(Subject subject, int selectedClassroomId)
        {
            if (ModelState.IsValid)
            {
                subject.Teacherid = selectedClassroomId;
                workBD.Subjects.Add(subject);
                await workBD.SaveChangesAsync();
                return RedirectToAction("MainImport");
            }
            var teachers = await workBD.Employees
                .Where(e => e.Role == 3)
                .ToListAsync();
            ViewBag.Teachers = teachers;
            return View(subject);
        }
        [HttpGet]
        public async Task<IActionResult> AddGroup()
        {
            var departments = await workBD.Departments.ToListAsync();
            var teachers = await workBD.Employees
                .Where(e => e.Role == 2 || e.Role == 3)
                .ToListAsync();

            ViewBag.Departments = departments;
            ViewBag.Teachers = teachers;

            return View(new Group());
        }

        [HttpPost]
        public async Task<IActionResult> AddGroup(Group group)
        {
            if (ModelState.IsValid)
            {
                if(group.Name != null)
                {
                    if (!string.IsNullOrEmpty(group.DepartmentId.ToString()))
                    {
                        if (string.IsNullOrEmpty(group.ClassroomTeacher.ToString()))
                        {
                            group.ClassroomTeacher = null;
                        }
                        workBD.Groups.Add(group);
                        await workBD.SaveChangesAsync();
                        return RedirectToAction("MainImport");
                    }                   
                }               
            }
            var departments = await workBD.Departments.ToListAsync();
            var teachers = await workBD.Employees
                .Where(e => e.Role == 2 || e.Role == 3)
                .ToListAsync();
            ViewBag.Departments = departments;
            ViewBag.Teachers = teachers;
            return View(group);
        }
        [HttpGet]
        public async Task<IActionResult> AddEmployeeGroup()
        {
            var groups = await workBD.Groups.Where(x => x.IsDepartment != 1).ToListAsync();
            var teachers = await workBD.Employees
                                        .Where(e => e.Role == 3)
                                        .ToListAsync();

            var model = new EmployeeGroupModel
            {
                GroupOptions = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                }).ToList(),

                TeacherOptions = teachers.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"{t.Surname} {t.Name} {t.Pat}"
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployeeGroup(EmployeeGroupModel model)
        {
            if (ModelState.IsValid)
            {
                if(model.GroupId != null)
                {
                    if(model.TeacherId != null)
                    {
                        var employeeGroup = new EmployeeGroup
                        {
                            GroupId = model.GroupId,
                            TeacherId = model.TeacherId
                        };

                        workBD.EmployeeGroups.Add(employeeGroup);
                        await workBD.SaveChangesAsync();
                        return RedirectToAction("MainImport");
                    }
                }             
            }

            var groups = await workBD.Groups.ToListAsync();
            var teachers = await workBD.Employees
                                        .Where(e => e.Role == 3 || e.Role == 2)
                                        .ToListAsync();

            model.GroupOptions = groups.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.Name
            }).ToList();

            model.TeacherOptions = teachers.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"{t.Surname} {t.Name} {t.Pat}"
            }).ToList();

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> AddStudent()
        {
            var groups = await workBD.Groups.Where(x => x.IsDepartment != 1).ToListAsync();

            var model = new AddStudentViewModel
            {
                GroupOptions = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddStudent(AddStudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Name != null)
                {
                    if (model.Surname != null)
                    {
                        if (model.Pat != null)
                        {
                            if (model.Login != null)
                            {
                                if (model.Password != null)
                                {
                                    if (model.Number != null)
                                    {
                                        if (model.GroupId != null)
                                        {
                                            var student = new Student
                                            {
                                                Surname = model.Surname,
                                                Name = model.Name,
                                                Pat = model.Pat,
                                                Login = model.Login,
                                                Password = model.Password,
                                                Number = model.Number,
                                                GroupDep = model.GroupId
                                            };

                                            workBD.Students.Add(student);
                                            await workBD.SaveChangesAsync();
                                            return RedirectToAction("MainImport");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }                          
            var groups = await workBD.Groups.Where(x => x.IsDepartment != 1).ToListAsync();
            model.GroupOptions = groups.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.Name
            }).ToList();
            return View(model);
        }
        public IActionResult EditStudent(int? id)
        {
            var students = workBD.Students.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Surname} {s.Name} {s.Pat}"
            }).ToList();

            var model = new EditStudentViewModel
            {
                StudentOptions = students
            };

            if (id.HasValue)
            {
                var student = workBD.Students.Find(id.Value);
                if (student != null)
                {
                    model.StudentId = student.Id;
                    model.Surname = student.Surname;
                    model.Name = student.Name;
                    model.Pat = student.Pat;
                    model.Login = student.Login;
                    model.Password = student.Password;
                    model.Number = student.Number;
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult EditStudent(EditStudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var student = workBD.Students.Find(model.StudentId);
                if (student != null)
                {
                    student.Surname = model.Surname;
                    student.Name = model.Name;
                    student.Pat = model.Pat;
                    student.Login = model.Login;
                    student.Password = model.Password;
                    student.Number = model.Number;
                    workBD.SaveChanges();
                }

                return RedirectToAction("MainImport");
            }

            model.StudentOptions = workBD.Students.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Surname} {s.Name} {s.Pat}"
            }).ToList();

            return View(model);
        }
        public IActionResult DeleteStudent()
        {
            var students = workBD.Students.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Surname} {s.Name} {s.Pat}"
            }).ToList();

            var model = new DeleteStudentViewModel
            {
                StudentOptions = students
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteStudent(int studentId)
        {
            var student = workBD.Students.Find(studentId);

            if (student == null)
            {
                return RedirectToAction("DeleteStudent");
            }
            var projectsToDelete = workBD.IndividualProjects.Where(p => p.Student == studentId);
            workBD.IndividualProjects.RemoveRange(projectsToDelete);
            workBD.Students.Remove(student);
            workBD.SaveChanges();
            return RedirectToAction("DeleteStudent");
        }

        public IActionResult DeleteEmployee()
        {
            var employees = workBD.Employees.Where(e => e.Role == 2 || e.Role == 3)
                                              .Select(e => new SelectListItem
                                              {
                                                  Value = e.Id.ToString(),
                                                  Text = $"{e.Surname} {e.Name} {e.Pat}"
                                              }).ToList();

            var model = new DeleteEmployeeViewModel
            {
                EmployeeOptions = employees
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteEmployee(int employeeId)
        {
            var employee = workBD.Employees.Find(employeeId);
            if (employee == null)
            {
                return RedirectToAction("DeleteEmployee");
            }
            if (employee.Role == 2)
            {
                var groupsToUpdate = workBD.Groups.Where(g => g.ClassroomTeacher == employeeId);
                foreach (var group in groupsToUpdate)
                {
                    group.ClassroomTeacher = null;
                }
            }
            else if (employee.Role == 3)
            {
                var employeeGroupsToDelete = workBD.EmployeeGroups.Where(eg => eg.TeacherId == employeeId);
                workBD.EmployeeGroups.RemoveRange(employeeGroupsToDelete);
                var subjectIDs = workBD.Subjects.Where(x => x.Teacherid == employeeId).Select(x => x.Id);
                foreach (var subjectID in subjectIDs)
                {
                    var projectsToUpdate = workBD.IndividualProjects.Where(p => p.Subject == subjectID);
                    foreach (var project in projectsToUpdate)
                    {
                        project.Subject = null;
                    }
                }
                var subjectsToDelete = workBD.Subjects.Where(s => s.Teacherid == employeeId);
                workBD.Subjects.RemoveRange(subjectsToDelete);                
            }
            workBD.Employees.Remove(employee);
            workBD.SaveChanges();
            return RedirectToAction("DeleteEmployee");
        }
        public IActionResult DeleteSubject()
        {
            var subjectInfos = (from s in workBD.Subjects
                                join t in workBD.Employees on s.Teacherid equals t.Id
                                select new SubjectViewModel
                                {
                                    Id = s.Id,
                                    Name = s.Name,
                                    TeacherFullName = $"{t.Surname} {t.Name} {t.Pat}"
                                }).ToList();

            var model = new SubjectViewModel
            {
                SubjectOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Выберите..." }
            }
            };

            model.SubjectOptions.AddRange(subjectInfos.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Name} ({s.TeacherFullName})"
            }));

            return View(model);
        }
        [HttpPost]
        public IActionResult DeleteSubject(int subjectId)
        {
            var subject = workBD.Subjects.FirstOrDefault(s => s.Id == subjectId);
            if (subject != null)
            {
                var projectsToUpdate = workBD.IndividualProjects.Where(p => p.Subject == subjectId);
                foreach (var project in projectsToUpdate)
                {
                    project.Subject = null;
                }
                workBD.Subjects.Remove(subject);
                workBD.SaveChanges();
            }
            return RedirectToAction("DeleteSubject");
        }
        public IActionResult DeleteGroup()
        {
            var groups = workBD.Groups.Where(x => x.IsDepartment != 1).Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.Name
            }).ToList();

            var model = new GroupViewModel
            {
                GroupOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Выберите группу..." }
            }
            };
            model.GroupOptions.AddRange(groups);

            return View(model);
        }
        [HttpPost]
        public IActionResult DeleteGroup(int groupId)
        {
            var group = workBD.Groups.FirstOrDefault(g => g.Id == groupId);
            if (group != null)
            {
                var students = workBD.Students.Where(s => s.GroupDep == groupId).ToList();
                foreach (var student in students)
                {
                    var projects = workBD.IndividualProjects.Where(p => p.Student == student.Id).ToList();
                    workBD.IndividualProjects.RemoveRange(projects);
                }
                workBD.Students.RemoveRange(students);
                var employeeGroups = workBD.EmployeeGroups.Where(eg => eg.GroupId == groupId).ToList();
                workBD.EmployeeGroups.RemoveRange(employeeGroups);
                workBD.Groups.Remove(group);

                workBD.SaveChanges();
            }
            return RedirectToAction("DeleteGroup");
        }
        public IActionResult DeleteGroupTeacher()
        {
            var groupTeacherInfos = (from eg in workBD.EmployeeGroups
                                     join g in workBD.Groups on eg.GroupId equals g.Id
                                     join t in workBD.Employees on eg.TeacherId equals t.Id
                                     select new GroupTeacherViewModel
                                     {
                                         GroupId = g.Id,
                                         TeacherId = t.Id,
                                         GroupName = g.Name,
                                         TeacherFullName = $"{t.Surname} {t.Name} {t.Pat}"
                                     }).ToList();

            var model = new GroupTeacherViewModel
            {
                GroupTeacherOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Выберите..." }
            }
            };
            model.GroupTeacherOptions.AddRange(groupTeacherInfos.Select(gt => new SelectListItem
            {
                Value = $"{gt.GroupId},{gt.TeacherId}",
                Text = $"{gt.GroupName} ({gt.TeacherFullName})"
            }));

            return View(model);
        }
        [HttpPost]
        public IActionResult DeleteGroupTeacher(string selectedGroupTeacher)
        {
            if (!string.IsNullOrEmpty(selectedGroupTeacher))
            {
                var ids = selectedGroupTeacher.Split(',');
                int groupId = int.Parse(ids[0]);
                int teacherId = int.Parse(ids[1]);

                var employeeGroup = workBD.EmployeeGroups.FirstOrDefault(eg => eg.GroupId == groupId && eg.TeacherId == teacherId);
                if (employeeGroup != null)
                {
                    workBD.EmployeeGroups.Remove(employeeGroup);
                    workBD.SaveChanges();
                }
            }
            return RedirectToAction("DeleteGroupTeacher");
        }
    }
}

