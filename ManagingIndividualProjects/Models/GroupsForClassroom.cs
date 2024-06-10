namespace ManagingIndividualProjects.Models
{
    public class GroupsForClassroom
    {
        public List<Group> Groups { get; set;}
        public List<Department> Departments { get; set;}
        public Dictionary<int, int> StudentCounts { get; set; }
    }
}
