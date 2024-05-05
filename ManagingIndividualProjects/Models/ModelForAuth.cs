using System.ComponentModel.DataAnnotations;

namespace ManagingIndividualProjects.Models
{
    public class ModelForAuth
    {
        [Required(ErrorMessage = "Не указано логин пользователя")]
        public string userName { get; set; }
        [Required(ErrorMessage = "Не указано пароль")]
        public string password { get; set; }

    }
}
