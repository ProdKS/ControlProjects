using System;
using System.Collections.Generic;

namespace ManagingIndividualProjects.Models;

public partial class User
{
    public long Id { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public string? Surname { get; set; }

    public string? NameAndPat { get; set; }

    public string? Number { get; set; }

    public long? Role { get; set; }

    public long? Theme { get; set; }

    public long? Department { get; set; }

    public long? GroupDep { get; set; }

    public virtual Department? DepartmentNavigation { get; set; }

    public virtual Group? GroupDepNavigation { get; set; }

    public virtual ICollection<IndividualProject> IndividualProjectStudentNavigations { get; set; } = new List<IndividualProject>();

    public virtual ICollection<IndividualProject> IndividualProjectTeacherNavigations { get; set; } = new List<IndividualProject>();

    public virtual Role? RoleNavigation { get; set; }

    public virtual IndividualProject? ThemeNavigation { get; set; }
}
