namespace ManagingIndividualProjects.Models
{
    public class GroupsViewModel
    {
        public List<Department> departments { get; set; }
        public List<Group> Groups { get; set; }
        public List<Employee> employees { get; set; }
        public Dictionary<int, int> StudentCounts { get; set; }
    }
}
