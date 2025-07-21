using Microsoft.EntityFrameworkCore;
using MTS.Data;
using System.Linq.Expressions;

namespace MTS.DAL.Repositories
{
	public interface IGenericRepository<T> where T : class
	{
		public Task AddAsync(T entity);
		public Task UpdateAsync(T entity);
		public Task DeleteAsyncById(object id);
		public Task DeleteAsync(params object[] keyValues);
		public Task<List<T>> GetAllByPropertyAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
		public Task<T?> GetByPropertyAsync(Expression<Func<T, bool>>? filter = null, bool tracked = true, string? includeProperties = null);
        Task AddRangeAsync(IEnumerable<T> entities); 
	}

	public class GenericRepository<T> : IGenericRepository<T> where T : class
	{
		private readonly MTS_Context _context;
		private readonly DbSet<T> _dbSet;

		public GenericRepository(MTS_Context context)
		{
			_context = context;
			_dbSet = _context.Set<T>();
		}

		public async Task AddAsync(T entity)
		{
			await _dbSet.AddAsync(entity);
		}

		public async Task UpdateAsync(T entity)
		{
			_dbSet.Update(entity);
		}

		public async Task DeleteAsyncById(object id)
		{
			var entity = await _dbSet.FindAsync(id);
			if (entity != null)
			{
				_dbSet.Remove(entity);
			}
		}
		public async Task DeleteAsync(params object[] keyValues)
		{
			var entity = await _dbSet.FindAsync(keyValues);
			if (entity != null)
			{
				_dbSet.Remove(entity);
			}
		}
		public async Task<List<T>> GetAllByPropertyAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
		{
			IQueryable<T> query = _dbSet;

			if (filter != null)
			{
				query = query.Where(filter);
			}
			if (includeProperties != null)
			{
				foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProp.Trim());
				}
			}

			return await query.ToListAsync();
		}

		public async Task<T?> GetByPropertyAsync(Expression<Func<T, bool>>? filter = null, bool tracked = true, string? includeProperties = null)
		{
			IQueryable<T> query = _dbSet;
			if (!tracked)
			{
				query = query.AsNoTracking();
			}
			// query = query.AsNoTracking();
			if (includeProperties != null)
			{
				foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProp.Trim());
				}
			}

			if (filter != null)
			{
				query = query.Where(filter);
			}
			return await query.FirstOrDefaultAsync();
		}

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

    }
}

