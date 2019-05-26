using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace SQLiteUniqueError
{
    public class UnitTest1
    {
        public class EntityA
        {
            public int EntityAId { get; set; }
            public EntityB EntityB { get; set; }
        }

        public class EntityB
        {
            public int EntityBId { get; set; }
        }

        public class MyContext : DbContext
        {
            public MyContext(DbContextOptions<MyContext> options) : base(options) { }
            public DbSet<EntityA> AEntities { get; set; }
            public DbSet<EntityB> BEntities { get; set; }
            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.Entity<EntityA>()
                    .HasOne(a => a.EntityB)
                    .WithOne()
                    .HasForeignKey<EntityA>("EntityBId")
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            }
        }

        [Fact]
        public void Error19UNIQUEconstraintfailed()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<MyContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new MyContext(options))
                {
                    context.Database.EnsureCreated();
                    //init data
                    context.BEntities.Add(new EntityB { EntityBId = 1 });
                    context.SaveChanges();
                }

                using (var context = new MyContext(options))
                {
                    context.Add(new EntityA { EntityAId = 1, EntityB = new EntityB { EntityBId = 1 } });
                    context.SaveChanges();
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public void SuccessRunningInSameUsing()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<MyContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new MyContext(options))
                {
                    context.Database.EnsureCreated();
                    //init data
                    context.BEntities.Add(new EntityB { EntityBId = 1 });
                    context.SaveChanges();

                    context.Add(new EntityA { EntityAId = 1, EntityB = new EntityB { EntityBId = 1 } });
                    context.SaveChanges();
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public void SuccessLoadingBEntities()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<MyContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new MyContext(options))
                {
                    context.Database.EnsureCreated();
                    //init data
                    context.BEntities.Add(new EntityB { EntityBId = 1 });
                    context.SaveChanges();
                }

                using (var context = new MyContext(options))
                {
                    context.BEntities.ToList();//workaround to avoid Unique error

                    context.Add(new EntityA { EntityAId = 1, EntityB = new EntityB { EntityBId = 1 } });
                    context.SaveChanges();
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }
}