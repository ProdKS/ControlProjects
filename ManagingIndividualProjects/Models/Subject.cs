using System;
using System.Collections.Generic;

namespace ManagingIndividualProjects.Models;

public partial class Subject
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<IndividualProject> IndividualProjects { get; set; } = new List<IndividualProject>();
}
