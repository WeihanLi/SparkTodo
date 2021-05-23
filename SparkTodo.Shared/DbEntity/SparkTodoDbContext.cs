using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SparkTodo.Models
{
    /// <summary>
    /// SparkTodoEntity
    /// </summary>
    public class SparkTodoDbContext : IdentityDbContext<UserAccount, UserRole, int>
    {
        public SparkTodoDbContext(DbContextOptions<SparkTodoDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Category>().HasKey(c => c.CategoryId);
            builder.Entity<TodoItem>().HasKey(t => t.TodoId);
            builder.Entity<SyncVersion>().HasKey(v => v.VersionId);

            builder.Entity<Category>().HasQueryFilter(f => f.IsDeleted == false);
            builder.Entity<TodoItem>().HasQueryFilter(f => f.IsDeleted == false);

            base.OnModelCreating(builder);
        }

        public virtual DbSet<Category> Categories { get; set; }

        public virtual DbSet<TodoItem> TodoItems { get; set; }

        public virtual DbSet<SyncVersion> SyncVersions { get; set; }
    }
}
