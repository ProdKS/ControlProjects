using System;
using System.Collections.Generic;

namespace ManagingIndividualProjects.Models;

public partial class Subject
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? Teacherid { get; set; }

    public virtual ICollection<IndividualProject> IndividualProjects { get; set; } = new List<IndividualProject>();

    public virtual Employee? Teacher { get; set; }
}
