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

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<IndividualProject> IndividualProjects { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=DESKTOP-EIOQ0DA\\SQLEXPRESS;Database=Projects;User Id=sa;password=sa;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });
        modelBuilder.Entity<Group>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsFixedLength();
        });
        modelBuilder.Entity<IndividualProject>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.NameTheme)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.IndividualProjects)
                .HasForeignKey(d => d.Status)
                .HasConstraintName("FK_IndividualProjects_Statuses");
            entity.HasOne(d => d.StudentNavigation).WithMany(p => p.IndividualProjectStudentNavigations)
                .HasForeignKey(d => d.Student)
                .HasConstraintName("FK_Student");
            entity.HasOne(d => d.SubjectNavigation).WithMany(p => p.IndividualProjects)
                .HasForeignKey(d => d.Subject)
                .HasConstraintName("FK_IndividualProjects_Subjects");
            entity.HasOne(d => d.TeacherNavigation).WithMany(p => p.IndividualProjectTeacherNavigations)
                .HasForeignKey(d => d.Teacher)
                .HasConstraintName("FK_Teacher");
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

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Login)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NameAndPat)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Number)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Surname)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.DepartmentNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.Department)
                .HasConstraintName("FK_Users_Departaments");

            entity.HasOne(d => d.GroupDepNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.GroupDep)
                .HasConstraintName("FK_Users_Groups");

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.Role)
                .HasConstraintName("FK_Users_Roles");

            entity.HasOne(d => d.ThemeNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.Theme)
                .HasConstraintName("FK_Users_Theme");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
