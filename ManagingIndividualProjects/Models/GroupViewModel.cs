using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManagingIndividualProjects.Models
{
    public class GroupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SelectListItem> GroupOptions { get; set; }
    }
}
