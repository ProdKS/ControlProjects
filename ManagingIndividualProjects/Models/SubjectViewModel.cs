using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManagingIndividualProjects.Models
{
    public class SubjectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SelectListItem> SubjectOptions { get; set; }
        public string TeacherFullName { get; set; }
    }
}
