using Microsoft.Extensions.Configuration;
using MTS.BLL.Services;
using MTS.BLL.Services.QRService;
using MTS.BLL.Services.VNPayService;
using MTS.DAL.Repositories;
using MTS.Data.Base;
using System.Net.Http;
using static MTS.BLL.Services.IUserService;

namespace MTS.BLL
{
	public interface IServiceProviders
	{
		IUserService UserService { get; }
		ITicketService TicketService { get; }
		IPaymentService PaymentService { get; }
		IPriorityApplicationService PriorityApplicationService { get; }
		IWalletService WalletService { get; }
		IRefundService RefundService { get; }
		IBusRouteService BusRouteService { get; }
		ITrainRouteService TrainRouteService { get; }
		ITerminalService TerminalService { get; }
		public class ServiceProviders : IServiceProviders
		{
			private readonly IUnitOfWork _unitOfWork;
			private readonly JWTSettings _jwtSettings;
			private readonly IConfiguration _configuration; 
			private readonly ISupabaseFileService _supabaseFileService;
			private readonly QRTokenGeneratorService _qrTokenGeneratorService;
			private readonly IHttpClientFactory _httpClientFactory;
			private readonly IVNPayRefundGatewayService _refundGatewayService;
			public ServiceProviders(IUnitOfWork unitOfWork, JWTSettings jwtSettings, IConfiguration configuration, ISupabaseFileService supabaseFileService, QRTokenGeneratorService qrTokenGeneratorService, IHttpClientFactory httpClientFactory)
			{
				_unitOfWork = unitOfWork;
				_jwtSettings = jwtSettings;
				_configuration = configuration;
				_supabaseFileService = supabaseFileService;
				_qrTokenGeneratorService = qrTokenGeneratorService;
				_httpClientFactory = httpClientFactory;
				_refundGatewayService = new VNPayRefundGatewayService(_configuration, _httpClientFactory);
			}

			public IUserService UserService => new UserService(_unitOfWork, _jwtSettings);

			public ITicketService TicketService => new TicketService(_unitOfWork);

			public IPaymentService PaymentService => new PaymentService(_configuration);

			public IPriorityApplicationService PriorityApplicationService => new PriorityApplicationService(_unitOfWork, _supabaseFileService);

			public IWalletService WalletService => new WalletService(_unitOfWork, _qrTokenGeneratorService);

			public IRefundService RefundService => new RefundService(_unitOfWork, _refundGatewayService, new WalletService(_unitOfWork, _qrTokenGeneratorService));
		}
			public IBusRouteService BusRouteService => new BusRouteService(_unitOfWork);

            public ITrainRouteService TrainRouteService => new TrainRouteService(_unitOfWork);

            public ITerminalService TerminalService => new TerminalService(_unitOfWork);
        }
	}
}
