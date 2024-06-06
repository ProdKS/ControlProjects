using System;
using System.Collections.Generic;

namespace ManagingIndividualProjects.Models;

public partial class Student
{
    public int Id { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public string? Surname { get; set; }

    public string? Name { get; set; }

    public string? Pat { get; set; }

    public string? Number { get; set; }

    public int? Role { get; set; }

    public int? GroupDep { get; set; }

    public virtual Group? GroupDepNavigation { get; set; }

    public virtual ICollection<IndividualProject> IndividualProjects { get; set; } = new List<IndividualProject>();

    public virtual Role? RoleNavigation { get; set; }
}
