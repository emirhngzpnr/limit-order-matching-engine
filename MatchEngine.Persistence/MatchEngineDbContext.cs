using MatchEngine.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine
{
    public class MatchEngineDbContext:DbContext
    {
        public MatchEngineDbContext(DbContextOptions<MatchEngineDbContext> options) : base(options)
        {

        }

        public DbSet<OrderCommandEntity> OrderCommands => Set<OrderCommandEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderCommandEntity>()
                .HasKey(command => command.SequenceNumber);
        }
    }
}
