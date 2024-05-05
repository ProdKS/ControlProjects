using System;
using System.Collections.Generic;

namespace ManagingIndividualProjects.Models;

public partial class Group
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public long DepartmentId { get; set; }
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual Department? DepartmentNavigation { get; set; }
}
