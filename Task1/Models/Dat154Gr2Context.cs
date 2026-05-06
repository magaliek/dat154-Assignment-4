using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Assignment_4.Models;

public partial class Dat154Gr2Context : DbContext
{
    public Dat154Gr2Context()
    {
    }

    public Dat154Gr2Context(DbContextOptions<Dat154Gr2Context> options)
        : base(options)
    {
    }

    public virtual DbSet<CustomUser> CustomUsers { get; set; }
    public virtual DbSet<Diagnosis> Diagnoses { get; set; }
    public virtual DbSet<Goal> Goals { get; set; }
    public virtual DbSet<Medication> Medications { get; set; }
    public virtual DbSet<Patient> Patients { get; set; }
    public virtual DbSet<Allergies> Allergies { get; set; }
    public virtual DbSet<SimulationSession> SimulationSessions { get; set; }
    public virtual DbSet<SimulationAction> SimulationActions { get; set; }
    public virtual DbSet<TeacherObservation> TeacherObservations { get; set; }
    public virtual DbSet<SimulationDeviation> SimulationDeviations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer("Server=tcp:dat154-gr2-assignment4.database.windows.net,1433;Initial Catalog=dat154_gr2;Persist Security Info=False;User ID=dat154ADMIN;Password=DAT154Assignment4Gr2;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_CustomUsers");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Role).HasMaxLength(50);
        });

        modelBuilder.Entity<Diagnosis>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_diagnoses");
            entity.ToTable("diagnoses");
            entity.Property(e => e.Diagnosis1).HasMaxLength(100).HasColumnName("diagnosis");
            entity.Property(e => e.PatientId).HasColumnName("Patient_id");
            entity.HasOne(d => d.Patient).WithMany(p => p.Diagnoses)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_diagnoses_Patients");
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Goals");
            entity.Property(e => e.Goal1).HasColumnName("Goal");
            entity.Property(e => e.PatientId).HasColumnName("Patient_id");
            entity.HasOne(d => d.Patient).WithMany(p => p.Goals)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Goals_Patients");
        });

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Medication");
            entity.ToTable("Medication");
            entity.Property(e => e.Medication1).HasMaxLength(100).HasColumnName("Medication");
            entity.Property(e => e.PatientId).HasColumnName("Patient_id");
            entity.HasOne(d => d.Patient).WithMany(p => p.Medications)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Medication_Patients");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Patients");
            entity.Property(e => e.Age).HasMaxLength(100).HasColumnName("age");
            entity.Property(e => e.SystolicBP).HasColumnName("SystolicBP");
            entity.Property(e => e.DiastolicBP).HasColumnName("DiastolicBP");
            entity.Property(e => e.HeartRate).HasColumnName("heart rate");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.OxygenSaturation).HasColumnName("oxygen saturation");
            entity.Property(e => e.RespiratoryRate).HasColumnName("respiratory rate");
            entity.Property(e => e.Sex).HasMaxLength(100).HasColumnName("sex");
            entity.Property(e => e.StudentId).HasColumnName("studentId");
            entity.Property(e => e.Temperature).HasColumnName("temperature");
            entity.Property(e => e.Weight).HasMaxLength(100).HasColumnName("weight");
        });

        modelBuilder.Entity<Allergies>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Allergies");
            entity.ToTable("Allergies");
            entity.Property(e => e.Allergy).HasMaxLength(100);
            entity.Property(e => e.PatientId).HasColumnName("Patient_id");
            entity.HasOne(d => d.Patient).WithMany(p => p.Allergies)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Allergies_Patients");
        });

        modelBuilder.Entity<SimulationSession>(entity =>
        {
            entity.ToTable("SimulationSessions");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Patient).WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SimulationAction>(entity =>
        {
            entity.ToTable("SimulationActions");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Session).WithMany(s => s.Actions)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TeacherObservation>(entity =>
        {
            entity.ToTable("TeacherObservations");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Session).WithMany(s => s.Observations)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SimulationDeviation>(entity =>
        {
            entity.ToTable("SimulationDeviations");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Action).WithMany(a => a.Deviations)
                .HasForeignKey(e => e.ActionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}