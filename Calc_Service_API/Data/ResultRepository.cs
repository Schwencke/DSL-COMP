using Events;

namespace Calc_Service_API.Data
{
    public class ResultRepository : IRepository<Result>
    {
        private readonly ResultContext db;

        public ResultRepository(ResultContext context)
        {
            db = context;
        }

        Result IRepository<Result>.Add(Result entity)
        {
            var newProduct = db.Results.Add(entity).Entity;
            db.SaveChanges();
            return newProduct;
        }


        Result IRepository<Result>.Get(Guid id)
        {
            return db.Results.FirstOrDefault(p => p.id == id);
        }

        IEnumerable<Result> IRepository<Result>.GetAll()
        {
            return db.Results.ToList();
        }

        void IRepository<Result>.Remove(Guid id)
        {
            var product = db.Results.FirstOrDefault(p => p.id == id);
            db.Results.Remove(product);
            db.SaveChanges();
        }
    }
}
