using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly DbContextOptions<ApplicationDbContext> _context;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> context) : base(context)

        {
            _context = context;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Worker>()
                .HasOne(x => x.Camp)
                .WithMany(x => x.Workers)
                .HasForeignKey(x => x.CampId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bed>()
               .HasOne(x => x.Room)
               .WithMany(x => x.Beds)
               .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Worker>()
               .HasOne(x => x.Bus)
               .WithMany(x => x.Workers)
               .HasForeignKey(x => x.BusId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rout_Stop>()
              .HasOne(x => x.Bus)
              .WithMany(x => x.Route_Stop)
              .HasForeignKey(x => x.BusId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Room>()
                .HasOne(x => x.Camp)
                .WithMany(x => x.Rooms)
                .HasForeignKey(x => x.CampId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasOne(x => x.Worker)
                .WithMany(x => x.Attendances)
                .HasForeignKey(x => x.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bus>()
                .HasOne(x => x.Driver)
                .WithMany(x => x.Bus)
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasOne(x => x.MarkedBy)
                .WithMany(x => x.Attendances)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DriverAttendance>()
                .HasOne(x => x.Driver)
                .WithMany(x => x.DriverAttendances)
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DriverAttendance>()
                .HasOne(x => x.MarkedBy)
                .WithMany(x => x.MarkedDriverAttendances)
                .HasForeignKey(x => x.MarkedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DriverAttendance>()
                .HasIndex(x => new { x.DriverId, x.AtDate })
                .IsUnique();

            modelBuilder.Entity<Leave>()
                .HasOne(x => x.User)
                .WithMany(x => x.Leaves)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Leave>()
                .HasOne(x => x.ApprovedByUser)
                .WithMany(x => x.ApprovedBy)
                .HasForeignKey(x => x.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Room>()
                .HasIndex(x => new { x.CampId, x.RoomNo })
                .IsUnique();
          
            modelBuilder.Entity<Supervisor>()
                .HasOne(x => x.User)
                .WithOne(x => x.Supervisor)
                .HasForeignKey<Supervisor>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Supervisor>()
                .HasOne(x => x.Camp)
                .WithMany(x => x.Supervisors)
                .HasForeignKey(x => x.CampId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<DriverAttendance> DriverAttendances { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<Bus> Buses { get; set; }
        public DbSet<Camp> Camps { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Rout_Stop> Rout_Stops { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }

    }
}
