using MTS.BLL.Services;
using MTS.DAL.Repositories;
using MTS.Data.Base;

namespace MTS.BLL
{
	public interface IServiceProviders
	{
		IUserService UserService { get; }
	}
	public class ServiceProviders : IServiceProviders
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly JWTSettings _jwtSettings;
		public ServiceProviders(IUnitOfWork unitOfWork, JWTSettings jwtSettings)
		{
			_unitOfWork = unitOfWork;
			_jwtSettings = jwtSettings;
		}

		public IUserService UserService => new UserService(_unitOfWork, _jwtSettings);
	}
}
