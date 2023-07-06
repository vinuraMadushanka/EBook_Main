using Models;

namespace DataAccess.Repository.IRepository
{
    public interface ICartDetailsRepository : IRepository<CartDetails>
    {
        void Update(CartDetails cartDetails);
    }
}