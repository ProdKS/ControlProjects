using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManagingIndividualProjects.Models
{
    public class DeleteEmployeeViewModel
    {
        public int EmployeeId { get; set; }
        public List<SelectListItem> EmployeeOptions { get; set; }
    }
}
