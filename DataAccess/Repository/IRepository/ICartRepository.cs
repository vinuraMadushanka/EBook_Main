using Models;

namespace DataAccess.Repository.IRepository
{
    public interface ICartRepository : IRepository<Carts>
    {
        void Update(Carts carts);
    }
}