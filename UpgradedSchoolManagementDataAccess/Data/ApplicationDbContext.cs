using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<SesseionTable> SesseionTables { get; set; }
        public DbSet<SchoolClasses> SchoolClasses { get; set; }
        public DbSet<SubClassTable> SubClassTables { get; set; }
        public DbSet<SubjectTable> SubjectTables { get; set; }
        public DbSet<AssessmentConfiguration> AssessmentConfigurations { get; set; }
        public DbSet<StudentsTable> StudentsTables { get; set; }
        public DbSet<EmployeeTable> EmployeeTables { get; set; }
        public DbSet<TermRegistration> TermRegistrations { get; set; }
        public DbSet<ResultTable> ResultTables { get; set; }
        //public DbSet<ResultScore> ResultScores { get; set; }
        public DbSet<StudentRating> StudentRatings { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<ParentGuardian> ParentGuardians { get; set; }
        public DbSet<StudentParentLink> StudentParentLinks { get; set; }
        public DbSet<PaymentCategory> PaymentCategories { get; set; }
        public DbSet<PaymentItem> PaymentItems { get; set; }
        public DbSet<PaymentSetup> PaymentSetups { get; set; }
        public DbSet<StudentPayment> StudentPayments { get; set; }
        public DbSet<StudentPaymentItem> StudentPaymentItems { get; set; }
        public DbSet<AppSettings> Appsettings { get; set; }
        public DbSet<TermGeneralInformation> TermGeneralInformations { get; set; }
        public DbSet<ClassTermInformation> ClassTermInformations { get; set; }
        public DbSet<ResultSkill> ResultSkills { get; set; }
        public DbSet<ClassResultSkill> ClassResultSkills { get; set; }
        public DbSet<StudentResultSkillRating> StudentResultSkillRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.Module);
            });

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

                entity.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ApplicationUserRole>(entity =>
            {
                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Role)
                    .WithMany(role => role.UserRoles)
                    .HasForeignKey(r => r.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<TermRegistration>()
                .HasOne(tr => tr.SchoolClasses)
                .WithMany()
                .HasForeignKey(tr => tr.SchoolClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TermRegistration>()
                .HasOne(tr => tr.SubClassTable)
                .WithMany()
                .HasForeignKey(tr => tr.SubClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TermRegistration>()
                .HasOne(tr => tr.SesseionTable)
                .WithMany()
                .HasForeignKey(tr => tr.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TermRegistration>()
                .HasOne(tr => tr.StudentsTable)
                .WithMany()
                .HasForeignKey(tr => tr.StudentId)
                .OnDelete(DeleteBehavior.Restrict);


            // ResultScore → ResultTable and AssessmentConfiguration
            //modelBuilder.Entity<ResultScore>(entity =>
            //{
            //    entity.HasOne(rs => rs.ResultTable)
            //        .WithMany(rt => rt.Scores)
            //        .HasForeignKey(rs => rs.ResultTableId)
            //        .OnDelete(DeleteBehavior.Cascade);

            //    entity.HasOne(rs => rs.AssessmentConfiguration)
            //        .WithMany(ac => ac.ResultScores)
            //        .HasForeignKey(rs => rs.AssessmentConfigId)
            //        .OnDelete(DeleteBehavior.Restrict);
            //});

            modelBuilder.Entity<StudentsTable>()
                .HasOne(st => st.ApplicationUser)
                .WithOne(a => a.Student)
                .HasForeignKey<StudentsTable>(st => st.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeTable>()
                .HasOne(et => et.ApplicationUser)
                .WithOne(a => a.Employee)
                .HasForeignKey<EmployeeTable>(et => et.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ParentGuardian entity
            modelBuilder.Entity<ParentGuardian>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Phone1).IsUnique();
                entity.HasIndex(p => p.Phone2).IsUnique();
                entity.Property(p => p.Phone1).IsRequired();
                entity.Property(p => p.FullName).IsRequired();
                entity.Property(p => p.Relationship).IsRequired();
            });

            // Configure StudentParentLink entity
            modelBuilder.Entity<StudentParentLink>(entity =>
            {
                entity.HasKey(spl => spl.Id);

                entity.HasOne(spl => spl.Student)
                    .WithMany()
                    .HasForeignKey(spl => spl.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(spl => spl.ParentGuardian)
                    .WithMany(p => p.StudentLinks)
                    .HasForeignKey(spl => spl.ParentGuardianId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Each student can have only one parent/guardian
                entity.HasIndex(spl => spl.StudentId).IsUnique();
            });

            // ── Payment Entity Relationships ──

            // PaymentSetup unique constraint: (PaymentItemId, SessionId, Term, SchoolClassId)
            modelBuilder.Entity<PaymentSetup>()
                .HasIndex(ps => new { ps.PaymentItemId, ps.SessionId, ps.Term, ps.SchoolClassId })
                .IsUnique();

            // PaymentItem → PaymentCategory (Restrict delete)
            modelBuilder.Entity<PaymentItem>()
                .HasOne(pi => pi.PaymentCategory)
                .WithMany(pc => pc.PaymentItems)
                .HasForeignKey(pi => pi.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // PaymentSetup → PaymentItem (Restrict delete)
            modelBuilder.Entity<PaymentSetup>()
                .HasOne(ps => ps.PaymentItem)
                .WithMany(pi => pi.PaymentSetups)
                .HasForeignKey(ps => ps.PaymentItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // PaymentSetup → SesseionTable
            modelBuilder.Entity<PaymentSetup>()
                .HasOne(ps => ps.SesseionTable)
                .WithMany()
                .HasForeignKey(ps => ps.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // PaymentSetup → SchoolClasses
            modelBuilder.Entity<PaymentSetup>()
                .HasOne(ps => ps.SchoolClass)
                .WithMany()
                .HasForeignKey(ps => ps.SchoolClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentPayment → TermRegistration (Restrict delete)
            modelBuilder.Entity<StudentPayment>()
                .HasOne(sp => sp.TermRegistration)
                .WithMany()
                .HasForeignKey(sp => sp.TermRegId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentPaymentItem → StudentPayment (Cascade delete)
            modelBuilder.Entity<StudentPaymentItem>()
                .HasOne(spi => spi.StudentPayment)
                .WithMany(sp => sp.PaymentItems)
                .HasForeignKey(spi => spi.StudentPaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentPaymentItem → PaymentItem (Restrict delete)
            modelBuilder.Entity<StudentPaymentItem>()
                .HasOne(spi => spi.PaymentItem)
                .WithMany()
                .HasForeignKey(spi => spi.PaymentItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassResultSkill>()
                .HasOne(crs => crs.SchoolClass)
                .WithMany()
                .HasForeignKey(crs => crs.SchoolClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassResultSkill>()
                .HasOne(crs => crs.ResultSkill)
                .WithMany(cr => cr.ClassResultSkills)
                .HasForeignKey(crs => crs.ResultSkillId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassResultSkill>()
                .HasIndex(crs => new { crs.SchoolClassId, crs.ResultSkillId })
                .IsUnique();

            modelBuilder.Entity<StudentResultSkillRating>()
                .HasOne(srr => srr.TermRegistration)
                .WithMany()
                .HasForeignKey(srr => srr.TermRegId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentResultSkillRating>()
                .HasOne(srr => srr.ResultSkill)
                .WithMany()
                .HasForeignKey(srr => srr.ResultSkillId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentResultSkillRating>()
                .HasIndex(srr => new { srr.TermRegId, srr.ResultSkillId })
                .IsUnique();
        }
    }
}