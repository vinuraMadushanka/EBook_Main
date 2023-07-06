using Models;

namespace DataAccess.Repository.IRepository
{
    public interface IBooksRepository : IRepository<Books>
    {
        void Update(Books books);
    }
}