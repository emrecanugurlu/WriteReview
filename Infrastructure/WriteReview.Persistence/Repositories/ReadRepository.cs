using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Repositories;
using WriteReview.Persistence.Contexts;

namespace WriteReview.Persistence.Repositories
{
    public class ReadRepository<T> : IReadRepository<T> where T : class
    {
        protected readonly WriteReviewDbContext _context;

        public ReadRepository(WriteReviewDbContext context)
        {
            _context = context;
        }

        public DbSet<T> Table => _context.Set<T>();

        public IQueryable<T> GetAll() => Table;
     

        public async Task<T> GetById(string id) 
            => await Table.FindAsync(Guid.Parse(id));

        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> method) 
            => await Table.FirstOrDefaultAsync(method);

        public IQueryable<T> GetWhere(Expression<Func<T, bool>> method) 
            => Table.Where(method);
    }
}
