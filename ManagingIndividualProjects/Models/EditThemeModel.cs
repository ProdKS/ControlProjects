using System.ComponentModel.DataAnnotations;

namespace ManagingIndividualProjects.Models
{
    public class EditThemeModel
    {
        public int idProject { get; set; }
        
        [Required(ErrorMessage = "Вы не указали тему")]
        public string nameTheme { get; set; }
        public string feedback { get; set; }
    }
}
