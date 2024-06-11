using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManagingIndividualProjects.Models
{
    public class EditStudentViewModel
    {
        public int StudentId { get; set; }
        public List<SelectListItem> StudentOptions { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Pat { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Number { get; set; }
    }
}