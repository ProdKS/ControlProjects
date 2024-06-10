using System.ComponentModel.DataAnnotations;

namespace ManagingIndividualProjects.Models
{
    public class AddFeedbackModel
    {
        public int idProject { get; set; }
        public string nameTheme { get; set; }
        [Required(ErrorMessage = "Отзыв не может быть пустым.")]
        public string feedback { get; set; }
    }

}
