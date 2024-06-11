using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManagingIndividualProjects.Models
{
    public class EmployeeGroupModel
    {
        public int GroupId { get; set; }
        public int? TeacherId { get; set; }
        public List<SelectListItem> GroupOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> TeacherOptions { get; set; } = new List<SelectListItem>();
    }
}
