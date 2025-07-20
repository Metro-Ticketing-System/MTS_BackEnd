using Microsoft.EntityFrameworkCore;
using MTS.Data;

namespace MTS.DAL.Repositories
{
	public interface IUnitOfWork
	{
		IGenericRepository<T> GetRepository<T>() where T : class;
		int SaveChangesWithTransaction();
		Task<int> SaveChangesWithTransactionAsync();
		Task<int> SaveAsync();
		void AttachAsUnchanged<TEntity>(TEntity entity) where TEntity : class;
    }
	public class UnitOfWork : IUnitOfWork
	{
		private readonly MTS_Context _context;

		public UnitOfWork() => _context ??= new MTS_Context();

		public IGenericRepository<T> GetRepository<T>() where T : class
		{
			return new GenericRepository<T>(_context);
		}

		public int SaveChangesWithTransaction()
		{
			int result = -1;

			//System.Data.IsolationLevel.Snapshot
			using (var dbContextTransaction = _context.Database.BeginTransaction())
			{
				try
				{
					result = _context.SaveChanges();
					dbContextTransaction.Commit();
				}
				catch (Exception)
				{
					//Log Exception Handling message                      
					result = -1;
					dbContextTransaction.Rollback();
				}
			}

			return result;
		}

		public async Task<int> SaveAsync()
		{
			return await _context.SaveChangesAsync();
		}

		public async Task<int> SaveChangesWithTransactionAsync()
		{
			int result = -1;

			//System.Data.IsolationLevel.Snapshot
			using (var dbContextTransaction = _context.Database.BeginTransaction())
			{
				try
				{
					result = await _context.SaveChangesAsync();
					dbContextTransaction.Commit();
				}
				catch (Exception)
				{
					//Log Exception Handling message                      
					result = -1;
					dbContextTransaction.Rollback();
				}
			}

			return result;
		}

        public void AttachAsUnchanged<TEntity>(TEntity entity) where TEntity : class
        {
            _context.Entry(entity).State = EntityState.Unchanged;
        }

    }
}
