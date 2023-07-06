using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            ApplicationRole = new ApplicationRoleRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            Books = new BooksRepository(_db);
            Order = new OrderRepository(_db);
            OrderDetails = new OrderDetailsRepository(_db);
            Cart = new CartRepository(_db);
            CartDetails = new CartDetailsRepository(_db);
            Comments = new CommentsRepository(_db);
            SP_Call = new SP_Call(_db);
        }

        public IApplicationRoleRepository ApplicationRole { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IBooksRepository Books { get; private set; }
        public IOrderRepository Order { get; private set; }
        public IOrderDetailsRepository OrderDetails { get; private set; }
        public ICartRepository Cart { get; private set; }
        public ICartDetailsRepository CartDetails { get; private set; }
        public ICommentsRepository Comments { get; private set; }
        public ISP_Call SP_Call { get; private set; }

        public void Dispose()
        {
            _db.Dispose();
        }

        public void SaveAsync()
        {
            _db.SaveChangesAsync().GetAwaiter().GetResult();
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _db.Database.BeginTransactionAsync();
        }
        public async void CommitAsync(IDbContextTransaction transaction)
        {
            await transaction.CommitAsync();
        }
        public async void RollbackAsync(IDbContextTransaction transaction)
        {
            await transaction.RollbackAsync();
        }
    }
}
