using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MTS.Data.Enums;
using MTS.Data.Models;

namespace MTS.Data
{
	public partial class MTS_Context : DbContext
	{
		public MTS_Context() { }

		public MTS_Context(DbContextOptions<MTS_Context> options) : base(options)
		{
		}

		public virtual DbSet<User> Users { get; set; }
		public virtual DbSet<Role> Roles { get; set; }

		public static string GetConnectionString(string connectionStringName)
		{
			var config = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddJsonFile("appsettings.json")
				.Build();

			string connectionString = config.GetConnectionString(connectionStringName);
			return connectionString;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
			=> optionsBuilder.UseSqlServer(GetConnectionString("DefaultConnection")).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

		#region Fluent API Configuration
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<User>()
				.HasOne(u => u.Role)
				.WithMany(r => r.Users)
				.HasForeignKey(u => u.RoleId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<User>()
				.HasIndex(u => u.UserName)
				.IsUnique();

			modelBuilder.Entity<User>()
				.HasIndex(u => u.Email)
				.IsUnique();

			// TicketType
			modelBuilder.Entity<TicketType>(entity =>
			{
				entity.HasKey(tt => tt.Id);
				entity.Property(tt => tt.TicketTypeName).IsRequired().HasMaxLength(100);
				entity.Property(tt => tt.Price).HasColumnType("decimal(18,2)");
			});

			// Terminal
			modelBuilder.Entity<Terminal>(entity =>
			{
				entity.HasKey(t => t.Id);
				entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
				entity.Property(t => t.Location).HasMaxLength(200);
			});

			// TrainRoute
			modelBuilder.Entity<TrainRoute>(entity =>
			{
				entity.HasKey(tr => tr.Id);
				entity.Property(tr => tr.Price).HasColumnType("decimal(18,2)");

				entity.HasOne(tr => tr.StartTerminalNavigation)
					  .WithMany(t => t.StartRoutes)
					  .HasForeignKey(tr => tr.StartTerminal)
					  .OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(tr => tr.EndTerminalNavigation)
					  .WithMany(t => t.EndRoutes)
					  .HasForeignKey(tr => tr.EndTerminal)
					  .OnDelete(DeleteBehavior.Restrict);
			});

			// Ticket
			modelBuilder.Entity<Ticket>(entity =>
			{
				entity.HasKey(t => t.Id);

				entity.Property(t => t.TotalAmount).HasColumnType("decimal(18,2)");
				entity.Property(t => t.QRCode).HasMaxLength(1024);
				entity.Property(t => t.ValidTo).IsRequired();

				entity.Property(t => t.Status)
					  .HasConversion<string>() // Enum to string
					  .IsRequired();

				entity.HasOne(t => t.TicketType)
					  .WithMany(tt => tt.Tickets)
					  .HasForeignKey(t => t.TicketTypeId)
					  .OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(t => t.TrainRoute)
					  .WithMany(tr => tr.Tickets)
					  .HasForeignKey(t => t.TrainRouteId)
					  .OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(t => t.Passenger)
					  .WithMany(u => u.Tickets)
					  .HasForeignKey(t => t.PassengerId)
					  .OnDelete(DeleteBehavior.Restrict);

			});

			#region Seed Data
			#region Role
			var roleAdmin = new Role
			{
				Id = 1,
				Name = "Admin",
				NormalizedName = "ADMIN"
			};

			var roleStaff = new Role
			{
				Id = 2,
				Name = "Staff",
				NormalizedName = "STAFF"
			};

			var rolePassenger = new Role
			{
				Id = 3,
				Name = "Passenger",
				NormalizedName = "PASSENGER"
			};

			modelBuilder.Entity<Role>().HasData(roleAdmin, roleStaff, rolePassenger);

			// Account
			var adminUser = new User
			{
				Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
				UserName = "Chaulb",
				NormalizedUserName = "CHAULB",
				Email = "Chaulbse182712@fpt.edu.vn",
				NormalizedEmail = "CHAULBSE182712@FPT.EDU.VN",
				EmailConfirmed = true,
				FirstName = "Chau",
				LastName = "Le Bao",
				DateOfBirth = DateOnly.Parse("2004-11-17"),
				CreatedAt = DateTime.Parse("2025-06-28"),
				IsActive = true,
				PasswordHash = "AQAAAAIAAYagAAAAENXGvXsCPKjc3lZglO1W7RUegv04aTnUvanOmCZwnJffvIlARmbRjHMJ155NXj4QWg==",
				//SecurityStamp = "STATIC-SECURITY-STAMP-A1",
				//ConcurrencyStamp = "STATIC-CONCURRENCY-STAMP-A1"
				RoleId = roleAdmin.Id
			};

			var staffUser1 = new User
			{
				Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
				UserName = "Anntp",
				NormalizedUserName = "ANNTP",
				Email = "Anntpse182743@fpt.edu.vn",
				NormalizedEmail = "ANNTPSE182743@FPT.EDU.VN",
				EmailConfirmed = true,
				FirstName = "An",
				LastName = "Nguyen Tran",
				DateOfBirth = DateOnly.Parse("2004-11-19"),
				CreatedAt = DateTime.Parse("2025-06-28"),
				IsActive = true,
				PasswordHash = "AQAAAAIAAYagAAAAENXGvXsCPKjc3lZglO1W7RUegv04aTnUvanOmCZwnJffvIlARmbRjHMJ155NXj4QWg==",
				//SecurityStamp = "STATIC-SECURITY-STAMP-S1",
				//ConcurrencyStamp = "STATIC-CONCURRENCY-STAMP-S1"
				RoleId = roleStaff.Id
			};

			var staffUser2 = new User
			{
				Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
				UserName = "Thaonn",
				NormalizedUserName = "THAONN",
				Email = "Thaonnse182709@fpt.edu.vn",
				NormalizedEmail = "THAONNSE182709@FPT.EDU.VN",
				EmailConfirmed = true,
				FirstName = "Thao",
				LastName = "Nguyen Ngoc",
				DateOfBirth = DateOnly.Parse("2004-12-23"),
				CreatedAt = DateTime.Parse("2025-06-28"),
				IsActive = true,
				PasswordHash = "AQAAAAIAAYagAAAAENXGvXsCPKjc3lZglO1W7RUegv04aTnUvanOmCZwnJffvIlARmbRjHMJ155NXj4QWg==",
				//SecurityStamp = "STATIC-SECURITY-STAMP-S2",
				//ConcurrencyStamp = "STATIC-CONCURRENCY-STAMP-S2"
				RoleId = roleStaff.Id
			};

			var passengerUser1 = new User
			{
				Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
				UserName = "Maint",
				NormalizedUserName = "MAINT",
				Email = "Maintse184085@fpt.edu.vn",
				NormalizedEmail = "MAINTSE184085@FPT.EDU.VN",
				EmailConfirmed = true,
				FirstName = "Mai",
				LastName = "Nguyen Thanh",
				DateOfBirth = DateOnly.Parse("2004-08-28"),
				CreatedAt = DateTime.Parse("2025-06-28"),
				IsActive = true,
				PasswordHash = "AQAAAAIAAYagAAAAENXGvXsCPKjc3lZglO1W7RUegv04aTnUvanOmCZwnJffvIlARmbRjHMJ155NXj4QWg==",
				//SecurityStamp = "STATIC-SECURITY-STAMP-P1",
				//ConcurrencyStamp = "STATIC-CONCURRENCY-STAMP-P1"
				RoleId = rolePassenger.Id
			};

			var passengerUser2 = new User
			{
				Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
				UserName = "Anhtd",
				NormalizedUserName = "ANHTD",
				Email = "Anhtdse184525@fpt.edu.vn",
				NormalizedEmail = "ANHTDSE184525@FPT.EDU.VN",
				EmailConfirmed = true,
				FirstName = "Anh",
				LastName = "Ta Duy",
				DateOfBirth = DateOnly.Parse("2004-08-06"),
				CreatedAt = DateTime.Parse("2025-06-28"),
				IsActive = true,
				PasswordHash = "AQAAAAIAAYagAAAAENXGvXsCPKjc3lZglO1W7RUegv04aTnUvanOmCZwnJffvIlARmbRjHMJ155NXj4QWg==",
				//SecurityStamp = "STATIC-SECURITY-STAMP-P2",
				//ConcurrencyStamp = "STATIC-CONCURRENCY-STAMP-P2"
				RoleId = rolePassenger.Id
			};

			modelBuilder.Entity<User>().HasData(adminUser, staffUser1, staffUser2, passengerUser1, passengerUser2);
			#endregion

			#region TicketType
			var ticketType1 = new TicketType
			{
				Id = 1,
				TicketTypeName = "Standard",
				Price = 150000m
			};

			var ticketType2 = new TicketType
			{
				Id = 2,
				TicketTypeName = "VIP",
				Price = 300000m
			};

			var ticketType3 = new TicketType
			{
				Id = 3,
				TicketTypeName = "Student",
				Price = 100000m
			};

			modelBuilder.Entity<TicketType>().HasData(ticketType1, ticketType2, ticketType3);

            #endregion

            #region Terminal
            var terminal1 = new Terminal
            {
                Id = 1,
                Name = "Hà Nội Station",
                Location = "Hà Nội"
            };

            var terminal2 = new Terminal
            {
                Id = 2,
                Name = "Đà Nẵng Station",
                Location = "Đà Nẵng"
            };

            var terminal3 = new Terminal
            {
                Id = 3,
                Name = "TP.HCM Station",
                Location = "TP.HCM"
            };

            modelBuilder.Entity<Terminal>().HasData(terminal1, terminal2, terminal3);
            #endregion

            #region TrainRoute
            var trainRoute1 = new TrainRoute
			{
				Id = 1,
				Price = 350000m,
				StartTerminal = 1,
				EndTerminal = 2
			};

			var trainRoute2 = new TrainRoute
			{
				Id = 2,
				Price = 500000m,
				StartTerminal = 2,
				EndTerminal = 3
			};

			var trainRoute3 = new TrainRoute
			{
				Id = 3,
				Price = 450000m,
				StartTerminal = 1,
				EndTerminal = 3
			};

			modelBuilder.Entity<TrainRoute>().HasData(trainRoute1, trainRoute2, trainRoute3);

			#endregion

			#region Ticket
			var ticket1 = new Ticket
			{
				Id = 1,
				PassengerId = Guid.Parse("44444444-4444-4444-4444-444444444444"), // Maint
				TicketTypeId = 1, // Standard
				TrainRouteId = 1,
				TotalAmount = 150000m,
				ValidTo = DateTime.Parse("2025-07-15"),
				PurchaseTime = DateTime.Parse("2025-07-01T10:00:00"),
				QRCode = "QR001",
				Status = TicketStatus.UnUsed,
				NumberOfTicket = 1,
				isPaid = false
			};

			var ticket2 = new Ticket
			{
				Id = 2,
				PassengerId = Guid.Parse("55555555-5555-5555-5555-555555555555"), // Anhtd
				TicketTypeId = 2, // VIP
				TrainRouteId = 2,
				TotalAmount = 600000m,
				ValidTo = DateTime.Parse("2025-07-20"),
				PurchaseTime = DateTime.Parse("2025-07-01T11:30:00"),
				QRCode = "QR002",
				Status = TicketStatus.InUse,
				NumberOfTicket = 2,
				isPaid = false
			};

			var ticket3 = new Ticket
			{
				Id = 3,
				PassengerId = Guid.Parse("44444444-4444-4444-4444-444444444444"), // Maint
				TicketTypeId = 3, // Student
				TrainRouteId = 3,
				TotalAmount = 100000m,
				ValidTo = DateTime.Parse("2025-07-10"),
				PurchaseTime = DateTime.Parse("2025-07-01T09:15:00"),
				QRCode = "QR003",
				Status = TicketStatus.Disabled,
				NumberOfTicket = 1,
				isPaid = false
			};

			modelBuilder.Entity<Ticket>().HasData(ticket1, ticket2, ticket3);

            #endregion

            #endregion
        }
        #endregion
    }
}