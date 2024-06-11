using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ManagingIndividualProjects.Models
{
    public class AddStudentViewModel
    {
        [Required]
        public string Surname { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Pat { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Number { get; set; }

        [Required]
        public int? GroupId { get; set; }

        public List<SelectListItem> GroupOptions { get; set; } = new List<SelectListItem>();
    }
}
