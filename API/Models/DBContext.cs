using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    public class DBContext : DbContext
    {

        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {

        }

        public virtual DbSet<Accounts> Accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Accounts>().HasKey(x => x.UserName);

            modelBuilder.Seeding();
        }
    }


    public static class SeedData
    {
        public static void Seeding(this ModelBuilder builder)
        {
            builder.Entity<Accounts>().HasData(new Accounts()
            {
                UserName = "Admin",
                Password = "123"
            });
        }
    }
}