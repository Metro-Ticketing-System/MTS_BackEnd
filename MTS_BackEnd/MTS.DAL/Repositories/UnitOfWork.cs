using MTS.Data;

namespace MTS.DAL.Repositories
{
	public interface IUnitOfWork
	{
		IGenericRepository<T> GetRepository<T>() where T : class;
		int SaveChangesWithTransaction();
		Task<int> SaveChangesWithTransactionAsync();

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

		//public async Task SaveAsync()
		//{
		//	await _context.SaveChangesAsync();
		//}

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
	}
}
