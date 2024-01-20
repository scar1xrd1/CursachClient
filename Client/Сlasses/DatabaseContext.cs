using Client.Сlasses;
using Microsoft.EntityFrameworkCore;

namespace Client.Images
{
    public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DatabaseContext()
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        public bool IsLoginExist(string login)
        {
            if (Users != null && Users.Count() >= 1)
            {
                var user = Users.FirstOrDefault(u => u.Login == login);
                if (user == null) return false;
                else return true;
            }
            return false;
        }

        public bool IsUserDataSuccess(string login, string password)
        {
            if (Users != null && Users.Count() >= 1)
            {
                var user = Users.FirstOrDefault(u => u.Login == login);

                if (user != null && PasswordHasher.VerifyPassword(user.Password, password, user.PasswordSalt)) return true;
                else return false;
            }
            return false;
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();

            //modelBuilder.Entity<User>()
            //    .HasKey(u => u.Id);

            //modelBuilder.Entity<MyProcess>()
            //    .HasKey(p => p.Id);

            //modelBuilder.Entity<MyProcess>()
            //    .HasOne(p => p.User)
            //    .WithMany(u => u.AllProcesses)
            //    .HasForeignKey(p => p.UserId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=SQL6031.site4now.net;Initial Catalog=db_aa3fc6_maindb;User Id=db_aa3fc6_maindb_admin;Password=password123");
            }
        }
    }
}
