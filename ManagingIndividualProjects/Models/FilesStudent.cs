namespace ManagingIndividualProjects.Models
{
    public partial class FilesStudent
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
        public int? IndividualProjectId { get; set; }

        public virtual IndividualProject IndividualProject { get; set; }
    }
}
