using System;
using System.Collections.Generic;

namespace ManagingIndividualProjects.Models;

public partial class Department
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
}
