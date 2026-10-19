using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

// This tells C# to use our RaceDay Route model
// instead of the ASP.NET Core Route class.
using Route = RaceDayAPI.Models.Route;


namespace RaceDayAPI.Data
{
    // ============================================================
    // RACEDAY DATABASE CONTEXT
    // ============================================================
    // This class connects our six C# models to SQL Server.
    //
    // It configures:
    // - Database tables
    // - Primary keys
    // - Foreign keys
    // - Column types
    // - Unique constraints
    // - Default values
    //
    // These settings are based on our Part 1 SQL script.
    // ============================================================

    public class RaceDayDbContext : DbContext
    {
        // Constructor receives database settings from Program.cs.
        public RaceDayDbContext(
            DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        // ========================================================
        // DATABASE TABLES
        // ========================================================

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }


        // ========================================================
        // DATABASE CONFIGURATION
        // ========================================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // ====================================================
            // USER TABLE
            // ====================================================

            modelBuilder.Entity<User>(entity =>
            {
                // Original SQL table: dbo.User
                entity.ToTable("User", "dbo", table =>
                {
                    table.HasCheckConstraint(
                        "CK_User_Role",
                        "Role IN ('Organiser', 'Participant')");
                });

                entity.HasKey(u => u.UserID)
                    .HasName("PK_User");

                entity.Property(u => u.UserID)
                    .ValueGeneratedOnAdd();

                entity.Property(u => u.FirstName)
                    .HasColumnType("varchar(50)")
                    .IsRequired();

                entity.Property(u => u.LastName)
                    .HasColumnType("varchar(50)")
                    .IsRequired();

                entity.Property(u => u.Email)
                    .HasColumnType("varchar(100)")
                    .IsRequired();

                entity.Property(u => u.PasswordHash)
                    .HasColumnType("varchar(255)")
                    .IsRequired();

                entity.Property(u => u.Role)
                    .HasColumnType("varchar(20)")
                    .IsRequired();

                entity.Property(u => u.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()")
                    .IsRequired();

                // Email must be unique.
                entity.HasAlternateKey(u => u.Email)
                    .HasName("UQ_User_Email");
            });


            // ====================================================
            // EVENT TABLE
            // ====================================================

            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("Event", "dbo");

                entity.HasKey(e => e.EventID)
                    .HasName("PK_Event");

                entity.Property(e => e.EventID)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.OrganiserID)
                    .IsRequired();

                entity.Property(e => e.EventName)
                    .HasColumnType("varchar(100)")
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnType("varchar(500)")
                    .IsRequired(false);

                entity.Property(e => e.EventDate)
                    .HasColumnType("datetime")
                    .IsRequired();

                entity.Property(e => e.Location)
                    .HasColumnType("varchar(150)")
                    .IsRequired();

                entity.Property(e => e.EventType)
                    .HasColumnType("varchar(30)")
                    .IsRequired();

                entity.Property(e => e.RegistrationDeadline)
                    .HasColumnType("datetime")
                    .IsRequired();

                // Each Event belongs to one User (Organiser).
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.OrganiserID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Event_Organiser");
            });


            // ====================================================
            // CATEGORY TABLE
            // ====================================================

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category", "dbo");

                entity.HasKey(c => c.CategoryID)
                    .HasName("PK_Category");

                entity.Property(c => c.CategoryID)
                    .ValueGeneratedOnAdd();

                entity.Property(c => c.EventID)
                    .IsRequired();

                entity.Property(c => c.CategoryName)
                    .HasColumnType("varchar(100)")
                    .IsRequired();

                entity.Property(c => c.DistanceKm)
                    .HasColumnType("decimal(6,2)")
                    .IsRequired();

                entity.Property(c => c.EntryFee)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(c => c.MaxParticipants)
                    .IsRequired();

                // Each Category belongs to one Event.
                entity.HasOne<Event>()
                    .WithMany()
                    .HasForeignKey(c => c.EventID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Category_Event");
            });


            // ====================================================
            // ROUTE TABLE
            // ====================================================

            modelBuilder.Entity<Route>(entity =>
            {
                entity.ToTable("Route", "dbo");

                entity.HasKey(r => r.RouteID)
                    .HasName("PK_Route");

                entity.Property(r => r.RouteID)
                    .ValueGeneratedOnAdd();

                entity.Property(r => r.EventID)
                    .IsRequired();

                entity.Property(r => r.RouteName)
                    .HasColumnType("varchar(100)")
                    .IsRequired();

                entity.Property(r => r.DistanceKm)
                    .HasColumnType("decimal(6,2)")
                    .IsRequired();

                entity.Property(r => r.StartLocation)
                    .HasColumnType("varchar(150)")
                    .IsRequired();

                entity.Property(r => r.FinishLocation)
                    .HasColumnType("varchar(150)")
                    .IsRequired();

                entity.Property(r => r.RouteInfo)
                    .HasColumnType("varchar(500)")
                    .IsRequired(false);

                // Each Route belongs to one Event.
                entity.HasOne<Event>()
                    .WithMany()
                    .HasForeignKey(r => r.EventID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Route_Event");
            });


            // ====================================================
            // ENROLMENT TABLE
            // ====================================================

            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.ToTable("Enrolment", "dbo");

                entity.HasKey(e => e.EnrolmentID)
                    .HasName("PK_Enrolment");

                entity.Property(e => e.EnrolmentID)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ParticipantID)
                    .IsRequired();

                entity.Property(e => e.EventID)
                    .IsRequired();

                entity.Property(e => e.CategoryID)
                    .IsRequired();

                entity.Property(e => e.EnrolmentDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()")
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasColumnType("varchar(20)")
                    .IsRequired();

                entity.Property(e => e.RaceNumber)
                    .HasColumnType("varchar(20)")
                    .IsRequired();

                // Race numbers cannot be duplicated.
                entity.HasAlternateKey(e => e.RaceNumber)
                    .HasName("UQ_Enrolment_RaceNumber");

                // Participant relationship.
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.ParticipantID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Enrolment_Participant");

                // Event relationship.
                entity.HasOne<Event>()
                    .WithMany()
                    .HasForeignKey(e => e.EventID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Enrolment_Event");

                // Category relationship.
                entity.HasOne<Category>()
                    .WithMany()
                    .HasForeignKey(e => e.CategoryID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Enrolment_Category");
            });


            // ====================================================
            // RESULT TABLE
            // ====================================================

            modelBuilder.Entity<Result>(entity =>
            {
                entity.ToTable("Result", "dbo");

                entity.HasKey(r => r.ResultID)
                    .HasName("PK_Result");

                entity.Property(r => r.ResultID)
                    .ValueGeneratedOnAdd();

                entity.Property(r => r.EnrolmentID)
                    .IsRequired();

                entity.Property(r => r.FinishTime)
                    .HasColumnType("time")
                    .IsRequired();

                entity.Property(r => r.OverallPosition)
                    .IsRequired(false);

                entity.Property(r => r.CategoryPosition)
                    .IsRequired(false);

                entity.Property(r => r.RecordedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()")
                    .IsRequired();

                // Matches the SQL UNIQUE constraint.
                entity.HasAlternateKey(r => r.EnrolmentID)
                    .HasName("UQ_Result_Enrolment");

                // One enrolment can have zero or one result.
                entity.HasOne<Enrolment>()
                    .WithOne()
                    .HasForeignKey<Result>(r => r.EnrolmentID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Result_Enrolment");
            });
        }
    }
}
