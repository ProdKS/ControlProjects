using System;
using System.Collections.Generic;

namespace ManagingIndividualProjects.Models;

public partial class IndividualProject
{
    public long Id { get; set; }

    public string? NameTheme { get; set; }

    public long? Subject { get; set; }

    public long? Teacher { get; set; }

    public long? Student { get; set; }

    public long? Status { get; set; }

    public long? Gradle { get; set; }

    public virtual Status? StatusNavigation { get; set; }

    public virtual User? StudentNavigation { get; set; }

    public virtual Subject? SubjectNavigation { get; set; }

    public virtual User? TeacherNavigation { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
