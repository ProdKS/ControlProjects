namespace ManagingIndividualProjects.Models
{
    public class TeacherProjectsModel
    {
        public bool IsClassroom { get; set; }
        public List<IndividualProject> IndividualProjects { get; set; }
        public List<Subject> Subjects { get; set; }
        public List<Student> Students { get; set; }
        public List<FilesStudent> Files { get; set; }
    }
}
