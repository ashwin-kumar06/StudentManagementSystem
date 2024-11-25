using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<UserCredential> UserCredentials { get; set; }
        public DbSet<StudentDetails> StudentDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Credential)
                .WithOne(c => c.User)
                .HasForeignKey<UserCredential>(c => c.UserId);

            modelBuilder.Entity<StudentDetails>()
                .HasOne(s => s.User)
                .WithOne(u => u.StudentDetails)
                .HasForeignKey<StudentDetails>(u => u.StudentId); 

            modelBuilder.Entity<UserCredential>()
                .HasOne(u => u.User)
                .WithOne(s => s.Credential)
                .HasForeignKey<UserCredential>(s => s.UserId); 
        }

        
    }
}