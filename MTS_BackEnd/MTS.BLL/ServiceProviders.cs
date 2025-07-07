using Microsoft.Extensions.Configuration;
using MTS.BLL.Services;
using MTS.DAL.Repositories;
using MTS.Data.Base;
using static MTS.BLL.Services.IUserService;

namespace MTS.BLL
{
	public interface IServiceProviders
	{
		IUserService UserService { get; }
		ITicketService TicketService { get; }
		IPaymentService PaymentService { get; }
		IPriorityApplicationService PriorityApplicationService { get; }
		public class ServiceProviders : IServiceProviders
		{
			private readonly IUnitOfWork _unitOfWork;
			private readonly JWTSettings _jwtSettings;
			private readonly IConfiguration _configuration; 
			private readonly ISupabaseFileService _supabaseFileService;
			public ServiceProviders(IUnitOfWork unitOfWork, JWTSettings jwtSettings, IConfiguration configuration, ISupabaseFileService supabaseFileService)
			{
				_unitOfWork = unitOfWork;
				_jwtSettings = jwtSettings;
				_configuration = configuration;
				_supabaseFileService = supabaseFileService;
			}

			public IUserService UserService => new UserService(_unitOfWork, _jwtSettings);

			public ITicketService TicketService => new TicketService(_unitOfWork);

			public IPaymentService PaymentService => new PaymentService(_configuration);

			public IPriorityApplicationService PriorityApplicationService => new PriorityApplicationService(_unitOfWork, _supabaseFileService);
		}
	}
}
