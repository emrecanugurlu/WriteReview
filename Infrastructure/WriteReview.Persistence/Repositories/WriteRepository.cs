using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WriteReview.Application.Repositories;
using WriteReview.Persistence.Contexts;

namespace WriteReview.Persistence.Repositories
{
    public class WriteRepository<T> : IWriteRepository<T> where T : class
    {
        private readonly WriteReviewDbContext _context;

        public WriteRepository(WriteReviewDbContext context)
        {
            _context = context;
        }

        public DbSet<T> Table => _context.Set<T>();

        public async Task<bool> AddAsync(T model)
        {
            var entityEntry = await Table.AddAsync(model);
            return entityEntry.State == EntityState.Added;
            
        }

        public async Task<bool> AddRangeAsync(List<T> datas)
        {
            await Table.AddRangeAsync(datas);
            return true;
        }

        public bool Remove(T model)
        {
            var entityEntry = Table.Remove(model);
            return entityEntry.State == EntityState.Deleted;
        }

        public async Task<bool> Remove(string id)
        {
            var model = await Table.FindAsync(Guid.Parse(id));
            return Remove(model);
            
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
       

        public bool Update(T model)
        {
            var entityEntry = Table.Update(model);
            return entityEntry.State == EntityState.Modified;
   
        }
    }
}
