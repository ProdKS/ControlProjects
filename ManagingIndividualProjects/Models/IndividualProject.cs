using System;
using System.Collections.Generic;

namespace ManagingIndividualProjects.Models;

public partial class IndividualProject
{
    public int Id { get; set; }

    public string? NameTheme { get; set; }

    public int? Subject { get; set; }

    public int? Student { get; set; }

    public int? Status { get; set; }

    public int? Gradle { get; set; }

    public string? Feedback { get; set; }

    public virtual Status? StatusNavigation { get; set; }

    public virtual Student? StudentNavigation { get; set; }

    public virtual Subject? SubjectNavigation { get; set; }
}
