using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ManagingIndividualProjects.Models
{
    public class ModelForAddingProjects
    {
        [Required(ErrorMessage = "Вы не указали тему")]
        public string nameTheme { get; set; }
        [Required(ErrorMessage = "Вы не выбрали дисциплину")]
        public int SelectedOption { get; set; }

        public List<SelectListItem> SubjectOptions { get; set; }
    }
}
