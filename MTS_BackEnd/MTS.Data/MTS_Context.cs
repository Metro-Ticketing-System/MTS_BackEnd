using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

			#region Seed Data
			// Role
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
				//EmailConfirmed = true,
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
				//EmailConfirmed = true,
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
				//EmailConfirmed = true,
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
				//EmailConfirmed = true,
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
				//EmailConfirmed = true,
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
		}
		#endregion
	}
}