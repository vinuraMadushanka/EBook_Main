using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class BooksRepository : Repository<Books>, IBooksRepository
    {
        private readonly ApplicationDbContext _db;

        public BooksRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Books books)
        {
            var obj = _db.Books.FirstOrDefault(x => x.Id ==books.Id);
            if (obj != null)
            {
                obj = books;
            }
        }
    }
}