namespace ManagingIndividualProjects.Models
{
    public class DepartmentDetail
    {
        public string NameDepartment { get; set; }
        public List<Group> Groups { get; set; }
        public List<Employee> employees { get; set; }
        public Dictionary<int, int> StudentCounts { get; set; }
    }
}
