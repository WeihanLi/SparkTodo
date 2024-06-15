// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SparkTodo.Models;

/// <summary>
/// SparkTodoEntity
/// </summary>
public class SparkTodoDbContext(DbContextOptions<SparkTodoDbContext> options)
    : IdentityDbContext<UserAccount, UserRole, int>(options)
{
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

