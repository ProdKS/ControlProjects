using ManagingIndividualProjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Text.RegularExpressions;

namespace ManagingIndividualProjects.Controllers
{
    public class TeacherPageController : Controller
    {
        private ProjectsContext workBD;
        public TeacherPageController(ProjectsContext workbd)
        {
            this.workBD = workbd;
        }
        public async Task<IActionResult> TeacherPageAsync()
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            int nowUserId = Convert.ToInt32(HttpContext.Session.GetString("Id"));
            var hasClassroom = await workBD.Groups.AnyAsync(x => x.ClassroomTeacher == nowUserId);
            var subjects = await workBD.Subjects.ToListAsync();
            var students = await workBD.Students.ToListAsync();
            var files = workBD.FilesStudents.ToList();
            var takeSubjects = await workBD.Subjects.Where(x => x.Teacherid == nowUserId).ToListAsync();
            List<IndividualProject> individualProjectsList = new List<IndividualProject>();
            foreach (var subject in takeSubjects)
            {
                var individualProjects = await workBD.IndividualProjects.Where(x => x.Subject == subject.Id).ToListAsync();
                if (individualProjects.Any())
                {
                    individualProjectsList.AddRange(individualProjects);
                }
            }
            var model = new TeacherProjectsModel
            {
                IndividualProjects = individualProjectsList,
                Students = students,
                IsClassroom = hasClassroom,
                Subjects = subjects,
                Files = files,
                ProjectGroupStatus = new Dictionary<int, bool>()
            };
            foreach (var project in model.IndividualProjects)
            {
                var student = model.Students.FirstOrDefault(s => s.Id == project.Student);
                if (student != null)
                {
                    var count = model.IndividualProjects
                                     .Where(ip => ip.Subject == project.Subject &&
                                                  (ip.Status == 1 || ip.Status == 4 || ip.Status == 5) &&
                                                  model.Students.Any(s => s.Id == ip.Student && s.GroupDep == student.GroupDep))
                                     .Count();
                    model.ProjectGroupStatus[project.Id] = count >= 3;
                }
            }
            return View(model);
        }
        public async Task<IActionResult> AcceptProject(int IndividualProjectid)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var projectToUpdate = await workBD.IndividualProjects.FindAsync(IndividualProjectid);
            if (projectToUpdate != null)
            {
                projectToUpdate.Status = 1;
                await workBD.SaveChangesAsync();
            }
            return RedirectToAction("TeacherPage", "TeacherPage");
        }
        public async Task<IActionResult> DeclineProject(int IndividualProjectid)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var projectToUpdate = await workBD.IndividualProjects.FindAsync(IndividualProjectid);
            if (projectToUpdate != null)
            {
                projectToUpdate.Status = 2;
                await workBD.SaveChangesAsync();
            }
            return RedirectToAction("TeacherPage", "TeacherPage");
        }
        public async Task<IActionResult> EditProjectTheme(int IndividualProjectid)
        {
            var projectNow = await workBD.IndividualProjects.FindAsync(IndividualProjectid);
            if (projectNow == null)
            {
                return RedirectToAction("TeacherPage", "TeacherPage");
            }

            var model = new EditThemeModel
            {
                idProject = IndividualProjectid,
                nameTheme = projectNow.NameTheme,
                feedback = projectNow.Feedback
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditProjectTheme(EditThemeModel editThemeModel)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var projectToUpdate = await workBD.IndividualProjects.FindAsync(editThemeModel.idProject);
            if (projectToUpdate != null)
            {
                projectToUpdate.NameTheme = editThemeModel.nameTheme;
                if (!string.IsNullOrEmpty(editThemeModel.feedback))
                {
                    projectToUpdate.Feedback = editThemeModel.feedback;
                }
                projectToUpdate.Status = 5;
                await workBD.SaveChangesAsync();
            }
            return RedirectToAction("TeacherPage");
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
            return RedirectToAction("TeacherPage", "TeacherPage");
        }
        public async Task<IActionResult> goBack()
        {
            return RedirectToAction("TeacherPage");
        }
        public async Task<IActionResult> CheckEditFeedback(int idProject)
        {
            var individualProject = await workBD.IndividualProjects.FindAsync(idProject);
            if (individualProject == null)
            {
                return RedirectToAction("TeacherPage", "TeacherPage");
            }

            var model = new FeedbackViewModel
            {
                idProject = idProject,
                feedback = individualProject.Feedback,
                IndividualProject = individualProject
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditFeedback(FeedbackViewModel feedbackViewModel)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var projectToUpdate = await workBD.IndividualProjects.FindAsync(feedbackViewModel.idProject);

            if (projectToUpdate != null)
            {
                if (!string.IsNullOrEmpty(feedbackViewModel.feedback))
                {
                    projectToUpdate.Feedback = feedbackViewModel.feedback;
                    await workBD.SaveChangesAsync();
                }
                else
                {                    
                    ModelState.AddModelError("", "Отзыв не может быть пустым.");
                    return View("CheckEditFeedback", feedbackViewModel);
                }
            }
            return RedirectToAction("TeacherPage");
        }
        public async Task<IActionResult> AddFeedback(int idProject)
        {
            var projectNow = await workBD.IndividualProjects.FindAsync(idProject);
            if (projectNow == null)
            {
                return RedirectToAction("TeacherPage", "TeacherPage");
            }

            var model = new AddFeedbackModel
            {
                idProject = idProject,
                nameTheme = projectNow.NameTheme
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddFeedback(AddFeedbackModel addFeedbackModel)
        {
            ViewBag.CurrentRole = Convert.ToInt32(HttpContext.Session.GetString("Role"));
            var projectToUpdate = await workBD.IndividualProjects.FindAsync(addFeedbackModel.idProject);
            if (projectToUpdate != null)
            {
                if (!string.IsNullOrEmpty(addFeedbackModel.feedback))
                {
                    projectToUpdate.Feedback = addFeedbackModel.feedback;
                    await workBD.SaveChangesAsync();
                }
                else
                {
                    // Если отзыв пустой, оставайтесь на текущей странице с моделью и сообщением об ошибке
                    ModelState.AddModelError("", "Отзыв не может быть пустым.");
                    return View("AddFeedback", addFeedbackModel);
                }
            }
            return RedirectToAction("TeacherPage");
        }
        public async Task<IActionResult> AddEditGrade(int idProject)
        {
            var individualProject = await workBD.IndividualProjects.FindAsync(idProject);
            if (individualProject == null)
            {
                return RedirectToAction("TeacherPage");
            }

            var model = new EditGradeViewModel
            {
                idProject = idProject,
                Grade = individualProject.Gradle,
                feedback = individualProject.Feedback
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEditGrade(EditGradeViewModel model)
        {
            var individualProject = await workBD.IndividualProjects.FindAsync(model.idProject);
            if (individualProject == null)
            {
                return RedirectToAction("TeacherPage");
            }
            individualProject.Gradle = model.Grade;
            individualProject.Feedback = model.feedback;
            await workBD.SaveChangesAsync();

            return RedirectToAction("TeacherPage");
        }
        public async Task<IActionResult> DownloadFile(int projectID)
        {
            var individualProject = await workBD.IndividualProjects.FindAsync(projectID);
            var file = await workBD.FilesStudents.Where(x => x.IndividualProjectId == projectID).FirstOrDefaultAsync();
            if (file == null)
                return RedirectToAction("TeacherPage");
            return File(file.FileData, "application/zip", file.FileName);
        }
        public async Task<IActionResult> GenerateStatement(int projectID)
        {
            var project = await workBD.IndividualProjects.FindAsync(projectID);
            if (project == null)
            {
                return RedirectToAction("TeacherPage");
            }
            var discipline = await workBD.Subjects.FindAsync(project.Subject);
            if(discipline == null)
            {
                return RedirectToAction("TeacherPage");
            }
            var student = await workBD.Students.FindAsync(project.Student);
            if (student == null)
            {
                return RedirectToAction("TeacherPage");
            }

            var group = await workBD.Groups.FindAsync(student.GroupDep);
            if (group == null)
            {
                return RedirectToAction("TeacherPage");
            }

            using (var stream = new MemoryStream())
            {
                var pdf = new PdfDocument();
                var page = pdf.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Arial", 12);
                var boldFont = new XFont("Arial", 12);
                var rect = new XRect(50, 50, page.Width - 100, page.Height - 100);
                var rightMargin = 240;
                gfx.DrawString("Руководителю управления учебной работы", font, XBrushes.Black, new XRect(rect.Right - rightMargin, rect.Top, rightMargin, 20), XStringFormats.TopLeft);
                gfx.DrawString("Федоровой А.В.", font, XBrushes.Black, new XRect(rect.Right - rightMargin, rect.Top + 20, rightMargin, 20), XStringFormats.TopLeft);
                gfx.DrawString("студента гр. " + group.Name, font, XBrushes.Black, new XRect(rect.Right - rightMargin, rect.Top + 40, rightMargin, 20), XStringFormats.TopLeft);
                gfx.DrawString($"{student.Surname} {student.Name} {student.Pat}", font, XBrushes.Black, new XRect(rect.Right - rightMargin, rect.Top + 60, rightMargin, 20), XStringFormats.TopLeft);           
                gfx.DrawString("заявление", boldFont, XBrushes.Black, new XRect(rect.Left, rect.Top + 100, rect.Width, 20), XStringFormats.TopCenter);
                gfx.DrawString($"Прошу разрешить выполнение индивидуального проекта по дисциплине {discipline.Name}", font, XBrushes.Black, new XRect(rect.Left, rect.Top + 140, rect.Width, 20), XStringFormats.TopLeft);
                gfx.DrawLine(XPens.Black, rect.Left, rect.Top + 180, rect.Right, rect.Top + 180); // Увеличенное расстояние
                gfx.DrawString("Дата:", font, XBrushes.Black, new XRect(rect.Left, rect.Top + 200, rect.Width / 2, 20), XStringFormats.TopLeft);
                gfx.DrawString("Подпись:                       ", font, XBrushes.Black, new XRect(rect.Right - rect.Width / 2, rect.Top + 200, rect.Width / 2, 20), XStringFormats.TopRight);
                gfx.DrawString("Согласие преподавателя данной дисциплины: __________________________________", font, XBrushes.Black, new XRect(rect.Left, rect.Top + 240, rect.Width, 20), XStringFormats.TopLeft);
                gfx.DrawString("Подпись преподавателя: ____________________________________________________", font, XBrushes.Black, new XRect(rect.Left, rect.Top + 280, rect.Width, 20), XStringFormats.TopLeft);
                pdf.Save(stream);
                var fileName = $"Заявление_{student.Surname}_{student.Name}.pdf";
                return File(stream.ToArray(), "application/pdf", fileName);
            }
        }
    }
}
