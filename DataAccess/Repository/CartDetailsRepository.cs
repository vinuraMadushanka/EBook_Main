using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class CartDetailsRepository : Repository<CartDetails>, ICartDetailsRepository
    {
        private readonly ApplicationDbContext _db;

        public CartDetailsRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(CartDetails cartsDetails)
        {
            var obj = _db.CartDetails.FirstOrDefault(x => x.Id ==cartsDetails.Id);
            if (obj != null)
            {
                obj = cartsDetails;
            }
        }
    }
}