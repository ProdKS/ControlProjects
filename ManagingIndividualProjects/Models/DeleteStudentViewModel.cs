using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManagingIndividualProjects.Models
{
    public class DeleteStudentViewModel
    {
        public int StudentId { get; set; }
        public List<SelectListItem> StudentOptions { get; set; }
    }
}
