using System;
using System.Collections.Generic;
using Demo1.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Data;

public partial class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<UserQuestionLog> UserQuestionLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserQuestionLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserQues__3214EC07F63E3685");

            entity.HasIndex(e => e.AskedAt, "NonClusteredIndex-AskedAt");

            entity.Property(e => e.AskedAt).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
