using System;
using System.Collections.Generic;

namespace ManagingIndividualProjects.Models;

public partial class Group
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? IsDepartment { get; set; }

    public int? ClassroomTeacher { get; set; }

    public int? DepartmentId { get; set; }

    public virtual Employee? ClassroomTeacherNavigation { get; set; }

    public virtual Department? Department { get; set; }

    public virtual ICollection<EmployeeGroup> EmployeeGroups { get; set; } = new List<EmployeeGroup>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
