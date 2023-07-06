using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class CartRepository : Repository<Carts>, ICartRepository
    {
        private readonly ApplicationDbContext _db;

        public CartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Carts carts)
        {
            var obj = _db.Carts.FirstOrDefault(x => x.Id ==carts.Id);
            if (obj != null)
            {
                obj = carts;
            }
        }
    }
}