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
            int groupStudent = Convert.ToInt32(HttpContext.Session.GetString("Group"));
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            int idStudent = Convert.ToInt32(HttpContext.Session.GetString("Id"));

            var individualProject = await workBD.IndividualProjects.Where(x => x.Student == idStudent).ToListAsync();
            var files = workBD.FilesStudents.ToList();
            var listSubjects = workBD.Subjects.ToList();
            var listEmployees = workBD.Employees.ToList();

            var teacherIDs = (from eg in workBD.EmployeeGroups
                              where eg.GroupId == groupStudent
                              select eg.TeacherId).ToList();

            bool isTeacherBusy = (from ip in workBD.IndividualProjects
                                  join s in workBD.Students on ip.Student equals s.Id
                                  where ip.Status != 2 && s.GroupDep == groupStudent
                                  select ip).Count() >= 3;

            var model = new IndividualProjectModel
            {
                Subjects = listSubjects,
                IndividualProjects = individualProject,
                Files = files,
                IsTeacherBusy = isTeacherBusy // передача булевого значения о занятости преподавателя
            };

            return View(model);
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
                                    TeacherFullName = t.Surname + " " + t.Name + " " + t.Pat,
                                    ProjectCount = (from ip in workBD.IndividualProjects
                                                    join stu in workBD.Students on ip.Student equals stu.Id
                                                    where ip.Subject == s.Id && ip.Status != 2 && stu.GroupDep == groupStudent
                                                    select ip).Count() >= 3
                                }).ToList();
            var model = new IndividualProjectModel
            {
                SubjectOptions = new List<SelectListItem>()
            };

            foreach (var info in subjectInfos)
            {
                string result = $"{info.SubjectName} ({info.TeacherFullName})";
                string optionClass = info.ProjectCount ? "red" : "green";
                model.SubjectOptions.Add(new SelectListItem { Value = info.SubjectID.ToString(), Text = result });
                ViewData[info.SubjectID.ToString()] = optionClass;
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddProjectAsync(IndividualProjectModel addingProjects)
        {
            int idStudent = Convert.ToInt32(HttpContext.Session.GetString("Id"));        
            if(addingProjects.nameTheme != null && addingProjects.SelectedOption != 0 && addingProjects != null)
            {
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
            else
            {
                RedirectToAction("AddProject");
            }
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
            model.feedback = individualProject.Feedback;
            model.gradle = gradle;
            model.IndividualProject = individualProject;
            return View(model);
        }
        public async Task<IActionResult> goBack()
        {            
            return RedirectToAction("StudentPage");
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
            var projectsToDelete = workBD.IndividualProjects.Where(p => p.Id == projectID);       
            if (projectToRemove != null)
            {
                var projectIds = projectsToDelete.Select(p => p.Id).ToList();
                var filesToDelete = workBD.FilesStudents
                    .Where(f => f.IndividualProjectId.HasValue && projectIds.Contains(f.IndividualProjectId.Value))
                    .ToList();
                workBD.FilesStudents.RemoveRange(filesToDelete);
                workBD.IndividualProjects.Remove(projectToRemove);
                await workBD.SaveChangesAsync();
            }
            return RedirectToAction("StudentPage", "StudentPage");

        }
        public IActionResult EditTheme(int idproject)
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

            var project = workBD.IndividualProjects.FirstOrDefault(p => p.Id == idproject);

            var model = new IndividualProjectModel
            {
                SubjectOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Выберите..." }
                },
                idproject = idproject,
                nameTheme = project.NameTheme,
            };

            foreach (var info in subjectInfos)
            {
                string result = $"{info.SubjectName} ({info.TeacherFullName})";
                model.SubjectOptions.Add(new SelectListItem { Value = info.SubjectID.ToString(), Text = result });
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditTheme(IndividualProjectModel model)
        {
            int idStudent = Convert.ToInt32(HttpContext.Session.GetString("Id"));
            var projectToUpdate = await workBD.IndividualProjects.FindAsync(model.idproject);
            if (projectToUpdate != null)
            {
                projectToUpdate.NameTheme = model.nameTheme;
                await workBD.SaveChangesAsync();
            }
            return RedirectToAction("StudentPage");
        }
        public IActionResult AddFile(int idproject)
        {
            ViewBag.ProjectId = idproject;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddFile(IFormFile file, int projectId)
        {
            if (file == null || file.Length == 0)
                return RedirectToAction("AddFile"); ;
            var filesStudent = new FilesStudent
            {
                FileName = file.FileName,
                IndividualProjectId = projectId
            };
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                filesStudent.FileData = memoryStream.ToArray();
            }

            workBD.FilesStudents.Add(filesStudent);
            await workBD.SaveChangesAsync();
            return RedirectToAction("StudentPage");
        }
        public async Task<IActionResult> EditDeleteFile(int projectId)
        {
            var files = await workBD.FilesStudents
                .Where(f => f.IndividualProjectId == projectId)
                .ToListAsync();
            return View(files);
        }

        [HttpPost]
        public async Task<IActionResult> EditDeleteFile(int fileId, IFormFile file)
        {
            var existingFile = await workBD.FilesStudents.FindAsync(fileId);
            if (existingFile == null)
                RedirectToAction("EditDeleteFile");

            if (file != null && file.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    existingFile.FileName = file.FileName;
                    existingFile.FileData = memoryStream.ToArray();
                }
            }

            await workBD.SaveChangesAsync();
            return RedirectToAction("StudentPage");
        }

        public async Task<IActionResult> DownloadFile(int fileId)
        {
            var file = await workBD.FilesStudents.FindAsync(fileId);
            if (file == null)
                RedirectToAction("EditDeleteFile");
            return File(file.FileData, "application/zip", file.FileName);
        }

        public async Task<IActionResult> DeleteFile(int fileId)
        {
            var file = await workBD.FilesStudents.FindAsync(fileId);
            if (file == null)
                return RedirectToAction("StudentPage");
            workBD.FilesStudents.Remove(file);
            await workBD.SaveChangesAsync();
            return RedirectToAction("StudentPage");
        }
    }
}
