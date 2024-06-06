namespace ManagingIndividualProjects.Models
{
    public class StudentsView
    {
        public List<IndividualProject> IndividualProjects { get; set; }
        public List<Student> Students { get; set; }
        public string nameGroup { get; set; }
        public List<Group> groups { get; set; }
        public Dictionary<int, bool> DebtorStatus { get; set; }
    }
}
