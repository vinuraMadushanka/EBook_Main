using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class CommentsRepository : Repository<Comments>, ICommentsRepository
    {
        private readonly ApplicationDbContext _db;

        public CommentsRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Comments comments)
        {
            var obj = _db.Comments.FirstOrDefault(x => x.Id ==comments.Id);
            if (obj != null)
            {
                obj = comments;
            }
        }
    }
}