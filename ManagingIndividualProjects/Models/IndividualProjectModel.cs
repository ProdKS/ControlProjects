﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ManagingIndividualProjects.Models
{
    public class IndividualProjectModel
    {
        public List<FilesStudent> Files { get; set; }
        public List<IndividualProject> IndividualProjects { get; set; }
        public List<SelectListItem> SubjectOptions { get; set; }
        public List<Subject> Subjects { get; set; }
        public string subjectID { get; set; }
        public string nameTheme { get; set; }
        public string feedBack { get; set; }
        public int idproject { get; set; }
        public int SelectedOption { get; set; }
        public bool IsTeacherBusy { get; set;}
    }
}
