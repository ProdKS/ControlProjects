using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ManagingIndividualProjects.Models;

public partial class ProjectsContext : DbContext
{
    public ProjectsContext()
    {
    }

    public ProjectsContext(DbContextOptions<ProjectsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeGroup> EmployeeGroups { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<IndividualProject> IndividualProjects { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=DESKTOP-EIOQ0DA\\SQLEXPRESS;Database=Projects;User Id=sa;Password=sa;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employee");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Login)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Number)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Pat)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Surname)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.Role)
                .HasConstraintName("FK_Employee_Roles");
        });

        modelBuilder.Entity<EmployeeGroup>(entity =>
        {
            entity.ToTable("EmployeeGroup");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");

            entity.HasOne(d => d.Group).WithMany(p => p.EmployeeGroups)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_EmployeeGroup_EmployeeGroup");

            entity.HasOne(d => d.Teacher).WithMany(p => p.EmployeeGroups)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK_EmployeeGroup_EmployeeGroup1");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsDepartment).HasDefaultValue(0);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.ClassroomTeacherNavigation).WithMany(p => p.Groups)
                .HasForeignKey(d => d.ClassroomTeacher)
                .HasConstraintName("FK_Groups_Employee");

            entity.HasOne(d => d.Department).WithMany(p => p.Groups)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_Groups_Departments");
        });

        modelBuilder.Entity<IndividualProject>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Feedback)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.NameTheme)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.IndividualProjects)
                .HasForeignKey(d => d.Status)
                .HasConstraintName("FK_IndividualProjects_Statuses");

            entity.HasOne(d => d.StudentNavigation).WithMany(p => p.IndividualProjects)
                .HasForeignKey(d => d.Student)
                .HasConstraintName("FK_IndividualProjects_Students");

            entity.HasOne(d => d.SubjectNavigation).WithMany(p => p.IndividualProjects)
                .HasForeignKey(d => d.Subject)
                .HasConstraintName("FK_IndividualProjects_Subjects");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Login)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Number)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Pat)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Surname)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.GroupDepNavigation).WithMany(p => p.Students)
                .HasForeignKey(d => d.GroupDep)
                .HasConstraintName("FK_Students_Groups");

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.Students)
                .HasForeignKey(d => d.Role)
                .HasConstraintName("FK_Students_Roles");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Teacherid).HasColumnName("teacherid");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Subjects)
                .HasForeignKey(d => d.Teacherid)
                .HasConstraintName("FK_Subjects_Employee");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
