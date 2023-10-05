
using Events;

namespace Calc_Service_API.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(ResultContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Products
            if (context.Results.Any())
            {
                return;   // DB has been seeded
            }

            List<Result> results = new List<Result>
            {

            };

            context.Results.AddRange(results);
            context.SaveChanges();
        }
    }
}
