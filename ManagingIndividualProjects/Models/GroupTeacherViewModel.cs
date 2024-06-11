using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManagingIndividualProjects.Models
{
    public class GroupTeacherViewModel
    {
        public int GroupId { get; set; }
        public int TeacherId { get; set; }
        public string GroupName { get; set; }
        public string TeacherFullName { get; set; }
        public List<SelectListItem> GroupTeacherOptions { get; set; }
    }
}
