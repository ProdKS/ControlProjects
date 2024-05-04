using System;
using System.Collections.Generic;

namespace ManagingIndividualProjects.Models;

public partial class Role
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
