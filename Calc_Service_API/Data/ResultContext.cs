
using Events;
using Microsoft.EntityFrameworkCore;

namespace Calc_Service_API.Data
{
    public class ResultContext : DbContext
    {
        public ResultContext(DbContextOptions<ResultContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Result>().Ignore(r => r.Headers);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Result> Results { get; set; }
    }
}
