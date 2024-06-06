using System;
using System.Collections.Generic;

namespace ManagingIndividualProjects.Models;

public partial class Employee
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

    public virtual ICollection<EmployeeGroup> EmployeeGroups { get; set; } = new List<EmployeeGroup>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual Role? RoleNavigation { get; set; }

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
