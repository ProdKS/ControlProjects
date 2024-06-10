namespace ManagingIndividualProjects.Models
{
    public class FeedbackViewModel
    {
        public int id { get; set; }
        public string feedback { get; set; }    
        public IndividualProject IndividualProject { get; set; }
        public string status {  get; set; }
        public string gradle { get; set; }
        public int idProject { get; set; }
    }
}
