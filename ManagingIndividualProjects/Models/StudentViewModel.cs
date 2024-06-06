namespace ManagingIndividualProjects.Models
{
    public class StudentViewModel
    {
        public List<Models.Group> Groups { get; set; }
        public List<IndividualProject> individualProjects { get; set; }
        public List<Student> Students { get; set; }
        public List<Department> Departments { get; set; }
        public Dictionary<int, bool> DebtorStatus { get; set; }
    }
}
