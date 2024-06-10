namespace ManagingIndividualProjects.Models
{
    public class DepartmentListViewModel
    {
        public List<Department> Departments { get; set; }
        public Dictionary<int, int> groupsCounts { get; set; }
    }
}
