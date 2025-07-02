using Microsoft.Extensions.Configuration;
using MTS.BLL.Services;
using MTS.DAL.Repositories;
using MTS.Data.Base;

namespace MTS.BLL
{
	public interface IServiceProviders
	{
		IUserService UserService { get; }
		ITicketService TicketService { get; }
        IPaymentService PaymentService { get; }
	}
	public class ServiceProviders : IServiceProviders
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly JWTSettings _jwtSettings;
		private readonly IConfiguration _configuration;
		public ServiceProviders(IUnitOfWork unitOfWork, JWTSettings jwtSettings, IConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_jwtSettings = jwtSettings;
			_configuration = configuration;	
		}

		public IUserService UserService => new UserService(_unitOfWork, _jwtSettings);

        public ITicketService TicketService => new TicketService(_unitOfWork);

		public IPaymentService PaymentService => new PaymentService(_configuration);
    }
}
