using System;
using System.Collections.Generic;

namespace ManagingIndividualProjects.Models;

public partial class EmployeeGroup
{
    public int Id { get; set; }

    public int? TeacherId { get; set; }

    public int? GroupId { get; set; }

    public virtual Group? Group { get; set; }

    public virtual Employee? Teacher { get; set; }
}
