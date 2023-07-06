using Models;

namespace DataAccess.Repository.IRepository
{
    public interface ICommentsRepository : IRepository<Comments>
    {
        void Update(Comments comments);
    }
}