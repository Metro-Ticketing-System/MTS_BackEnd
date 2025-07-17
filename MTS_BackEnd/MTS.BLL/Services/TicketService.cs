// MTS.BLL/Services/TicketService.cs
using MTS.BLL.Services.QRService;
using MTS.DAL.Dtos;
using MTS.DAL.Repositories;
using MTS.Data.Enums;
using MTS.Data.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MTS.BLL.Services
{
	public interface ITicketService
	{
		Task<CreateTicketResponseDto> CreateTicket(CreateTicketRequestDto createTicketRequestDto);
		Task<TicketResponse?> GetTicketById(int id);
		Task<bool> DeleteTicket(int id);
		Task<CreateTicketResponseDto> UpdateTicket(TicketDto ticket);
		Task<CreateTicketResponseDto> CreatePriorityTicket(CreatePriorityTicketRequestDto request);
		Task<List<TicketResponse>> GetListTicket(Guid userId);
        Task<CreateTicketResponseDto> DisableTicket(int id);
        Task<CreateTicketResponseDto> ActiveTicket(int id);
		Task<string> GenerateQRToken(Guid userId, int ticketId);
		Task<QRScanResponse> QRScan(QRScanRequest request);
        Task SendPushNotification(Guid userId, int ticketId);
        Task SendExpoPushAsync(string expoPushToken, string title, string body, object? data = null);
        Task<TicketResponse?> CheckTicketExpire(int ticketId);
    }

    public class TicketService : ITicketService
	{
		private readonly IUnitOfWork _unitOfWork;
		private IGenericRepository<Ticket> _ticketRepo;
        private readonly QRTokenGeneratorService _qRTokenGeneratorService;

        public TicketService(IUnitOfWork unitOfWork, QRTokenGeneratorService qRTokenGeneratorService)
		{
			_unitOfWork = unitOfWork;
			_ticketRepo = _unitOfWork.GetRepository<Ticket>();
            _qRTokenGeneratorService = qRTokenGeneratorService;
        }

		public async Task<CreateTicketResponseDto> CreateTicket(CreateTicketRequestDto createTicketRequestDto)
		{
			try
			{
				var ticket = new Ticket
				{
					CreatedBy = createTicketRequestDto.PassengerId.ToString(),
					CreatedTime = DateTime.UtcNow,
					LastUpdatedTime = DateTime.UtcNow,
					PassengerId = createTicketRequestDto.PassengerId,
					TicketTypeId = createTicketRequestDto.TicketTypeId,
					TotalAmount = createTicketRequestDto.TotalAmount,
					ValidTo = DateTime.UtcNow.AddDays(1),
					TrainRouteId = createTicketRequestDto.TrainRouteId ?? null,
					Status = Data.Enums.TicketStatus.UnUsed,
					NumberOfTicket = createTicketRequestDto.NumberOfTicket,
				};
				await _ticketRepo.AddAsync(ticket);
				var succeedCount = await _unitOfWork.SaveAsync();
				if (succeedCount > 0)
				{
					return new CreateTicketResponseDto
					{
						IsSuccess = true,
						TicketId = ticket.Id
					};
				}
				return new CreateTicketResponseDto { IsSuccess = false };
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during ticket creation: {ex.Message}");
				return null;
			}
		}

		public async Task<bool> DeleteTicket(int id)
		{
			try
			{
				await _ticketRepo.DeleteAsync(id);
				await _unitOfWork.SaveAsync();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during ticket deletion: {ex.Message}");
				return false;
			}
		}

		public async Task<TicketResponse?> GetTicketById(int id)
		{
			try
			{
				var ticket = await _ticketRepo.GetByPropertyAsync(t => t.Id == id & t.IsDeleted == false, includeProperties: "Passenger, TrainRoute, TicketType");
				if (ticket == null)
				{
					return null;
				}
				var dto = new TicketResponse
                {
                    TicketId = ticket.Id,
                    PassengerId = ticket.PassengerId,
                    PassengerName = ticket.Passenger.FirstName + " " + ticket.Passenger.LastName,
                    Email = ticket.Passenger.Email,
                    TicketTypeId = ticket.TicketTypeId,
                    TicketTypeName = ticket.TicketType.TicketTypeName,
                    TotalAmount = ticket.TotalAmount,
                    ValidTo = ticket.ValidTo,
                    PurchaseTime = ticket.PurchaseTime,
                    TrainRouteId = ticket.TrainRouteId ?? 0,
                    TrainRoutePrice = ticket.TrainRoute?.Price ?? 0m,
                    StartTerminal = ticket.TrainRoute?.StartTerminal ?? null,
                    EndTerminal = ticket.TrainRoute?.EndTerminal ?? null,
                    QRCode = ticket.QRCode,
                    Status = ticket.Status,
                    NumberOfTicket = ticket.NumberOfTicket,
                    IsPaid = ticket.isPaid,
                    TxnRef = ticket.TxnRef,
                    VnPayTransactionDate = ticket.VnPayTransactionDate,
                    VnPayTransactionNo = ticket.VnPayTransactionNo
                };
				return dto;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting ticket by id: {ex.Message}");
				return null;
			}
		}

		public async Task<CreateTicketResponseDto> UpdateTicket(TicketDto ticket)
		{
			try
			{
				var ticketModel = await _ticketRepo.GetByPropertyAsync(t => t.Id == ticket.TicketId);
				if (ticketModel == null)
				{
					return new CreateTicketResponseDto { IsSuccess = false };
				}
                ticketModel.ValidTo = ticket.ValidTo;
				ticketModel.Status = ticket.Status;
				ticketModel.isPaid = ticket.isPaid;
				ticketModel.QRCode = ticket.QRCode;
				ticketModel.LastUpdatedTime = DateTime.UtcNow;
				ticketModel.PurchaseTime = ticket.PurchaseTime;
				ticketModel.TxnRef = ticket.TxnRef;
				ticketModel.VnPayTransactionNo = ticket.VnPayTransactionNo;
				ticketModel.VnPayTransactionDate = ticket.VnPayTransactionDate;
				await _ticketRepo.UpdateAsync(ticketModel);
				var succeedCount = await _unitOfWork.SaveAsync();
				if (succeedCount > 0)
				{
					return new CreateTicketResponseDto
					{
						IsSuccess = true,
						TicketId = ticketModel.Id,
					};
				}
				return new CreateTicketResponseDto { IsSuccess = false };

			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during ticket update: {ex.Message}");
				return null;
			}
		}

        public async Task<CreateTicketResponseDto> CreatePriorityTicket(CreatePriorityTicketRequestDto request)
        {
            try
            {
                var ticket = new Ticket
                {
                    CreatedBy = request.PassengerId.ToString(),
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow,
                    PassengerId = request.PassengerId,
                    TicketTypeId = request.TicketTypeId,
                    TotalAmount = 0,
                    ValidTo = DateTime.UtcNow.AddYears(1),
                    TrainRouteId = request.TrainRouteId,
                    Status = Data.Enums.TicketStatus.UnUsed,
                    NumberOfTicket = 1,
					PurchaseTime = DateTime.UtcNow,
					isPaid = true,
                };
                await _ticketRepo.AddAsync(ticket);
                var succeedCount = await _unitOfWork.SaveAsync();
                if (succeedCount > 0)
                {
					ticket.QRCode = _qRTokenGeneratorService.GenerateQRToken(ticket.Id, ticket.PassengerId);
                    await _ticketRepo.UpdateAsync(ticket);
                    await _unitOfWork.SaveAsync();

                    return new CreateTicketResponseDto
                    {
                        IsSuccess = true,
                        TicketId = ticket.Id
                    };
                }
                return new CreateTicketResponseDto { IsSuccess = false };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during ticket creation: {ex.Message}");
                return null;
            }
        }

        public async Task<List<TicketResponse>> GetListTicket(Guid userId)
        {
            try
            {
				var tickets = await _ticketRepo.GetAllByPropertyAsync(t => t.PassengerId == userId, includeProperties: "Passenger, TrainRoute, TicketType");
				var result = tickets.Select(t => new TicketResponse
				{
					TicketId = t.Id,
					PassengerId = t.PassengerId,
					PassengerName = t.Passenger.FirstName + " " + t.Passenger.LastName,
					Email = t.Passenger.Email,
					TicketTypeId = t.TicketTypeId,
					TicketTypeName = t.TicketType.TicketTypeName,
					TotalAmount = t.TotalAmount,
					ValidTo = t.ValidTo,
					PurchaseTime = t.PurchaseTime,
					TrainRouteId = t.TrainRouteId ?? 0,
                    TrainRoutePrice = t.TrainRoute?.Price ?? 0m,
                    StartTerminal = t.TrainRoute?.StartTerminal ?? null,
                    EndTerminal = t.TrainRoute?.EndTerminal ?? null,
                    QRCode = t.QRCode,
					Status = t.Status,
					NumberOfTicket = t.NumberOfTicket ?? 0,
					IsPaid = t.isPaid,
					TxnRef = t.TxnRef,
					VnPayTransactionDate = t.VnPayTransactionDate,
					 VnPayTransactionNo = t.VnPayTransactionNo
				}
				).ToList();

				return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }

        public async Task<CreateTicketResponseDto> DisableTicket(int id)
        {
            try
            {
				var ticket = await _ticketRepo.GetByPropertyAsync(t => t.Id == id);
				if(ticket == null)
				{
                    return new CreateTicketResponseDto { IsSuccess = false };
                }
				ticket.Status = Data.Enums.TicketStatus.Disabled;
                await _ticketRepo.UpdateAsync(ticket);
                var succeedCount = await _unitOfWork.SaveAsync();
                if (succeedCount > 0)
                {
                    return new CreateTicketResponseDto
                    {
                        IsSuccess = true,
                        TicketId = ticket.Id,
                    };
                }
                return new CreateTicketResponseDto { IsSuccess = false };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }

        public async Task<CreateTicketResponseDto> ActiveTicket(int id)
        {
            try
            {
                var ticket = await _ticketRepo.GetByPropertyAsync(t => t.Id == id);
                if (ticket == null)
                {
                    return new CreateTicketResponseDto { IsSuccess = false };
                }

                bool isOneWay = ticket.TicketTypeId == 1;
                if(isOneWay)
                {
                    ticket.ValidTo = DateTime.UtcNow.AddDays(1);
                }    

                ticket.Status = Data.Enums.TicketStatus.UnUsed;
                await _ticketRepo.UpdateAsync(ticket);
                var succeedCount = await _unitOfWork.SaveAsync();
                if (succeedCount > 0)
                {
                    return new CreateTicketResponseDto
                    {
                        IsSuccess = true,
                        TicketId = ticket.Id,
                    };
                }
                return new CreateTicketResponseDto { IsSuccess = false };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GenerateQRToken(Guid userId, int ticketId)
        {
            try
            {
				var token = _qRTokenGeneratorService.GenerateQRToken(ticketId, userId);
				return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }

        public async Task<QRScanResponse> QRScan(QRScanRequest request)
        {
            try
            {
                var principal = _qRTokenGeneratorService.ValidateQRToken(request.QRToken);
                if (principal == null)
                {
                    return new QRScanResponse { Message = "QR không hợp lệ hoặc đã hết hạn." };
                }

                var ticketId = int.Parse(principal.FindFirst("ticket_id")!.Value);
                var userId = Guid.Parse(principal.FindFirst("user_id")!.Value);

                var ticket = await _ticketRepo.GetByPropertyAsync(t => t.Id == ticketId, includeProperties: "Passenger, TrainRoute, TicketType");
                if (ticket == null || ticket.PassengerId != userId)
                {
                    return new QRScanResponse { Message = "Không tìm thấy vé hoặc thông tin người dùng không khớp." };
                }

                if (ticket.isPaid == false)
                {
                    return new QRScanResponse
                    {
                        Message = "Vé chưa được thanh toán."
                    };
                }

                if (ticket.ValidTo < DateTime.UtcNow && ticket.Status == TicketStatus.UnUsed)
                {
                    return new QRScanResponse
                    {
                        Message = "Vé đã hết hạn sử dụng."
                    };
                }

                bool isOneWay = ticket.TicketTypeId == 1;

                // Validate theo hướng đi
                if (isOneWay)
                {
                    if (!request.isOut)
                    {
                        // Check-in: Vé phải UnUsed & TerminalId khớp điểm xuất phát
                        if (ticket.Status != TicketStatus.UnUsed)
                            return new QRScanResponse { Message = "Vé đã được sử dụng hoặc không hợp lệ để vào cổng." };

                        if (request.TerminalId != ticket.TrainRoute.StartTerminal)
                            return new QRScanResponse { Message = "Bạn không ở đúng điểm bắt đầu để sử dụng vé." };
                    }
                    else
                    {
                        // Check-out: Vé phải InUse & TerminalId khớp điểm đến
                        if (ticket.Status != TicketStatus.InUse)
                            return new QRScanResponse { Message = "Vé chưa được sử dụng để vào cổng, không thể ra." };

                        if (request.TerminalId != ticket.TrainRoute.EndTerminal)
                            return new QRScanResponse { Message = "Bạn không ở đúng điểm đến để kết thúc hành trình." };
                    }
                }
                else
                {
                    if (!request.isOut)
                    {
                        // Check-in: Vé phải UnUsed
                        if (ticket.Status != TicketStatus.UnUsed)
                            return new QRScanResponse { Message = "Vé đã được sử dụng hoặc không hợp lệ để vào cổng." };
                    }
                    else
                    {
                        // Check-out: Vé phải InUse
                        if (ticket.Status != TicketStatus.InUse)
                            return new QRScanResponse { Message = "Vé chưa được sử dụng để vào cổng, không thể ra." };
                    }
                }

                // Update trạng thái
                if (!request.isOut)
                {
                    ticket.Status = TicketStatus.InUse;
                }
                else
                {
                    ticket.Status = isOneWay ? TicketStatus.Disabled : TicketStatus.UnUsed;
                }

                if(isOneWay & request.isOut)
                {
                    ticket.ValidTo = DateTime.UtcNow.AddSeconds(1);
                }
                ticket.LastUpdatedTime = DateTime.UtcNow;
                await _ticketRepo.UpdateAsync(ticket);
                await _unitOfWork.SaveAsync();

                return new QRScanResponse
                {
                    UserId = ticket.PassengerId,
                    TicketId = ticket.Id,
                    NumberOfTicket = ticket.NumberOfTicket ?? 0,
                    Message = "Quét mã thành công."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during QR scan: {ex.Message}");
                return new QRScanResponse { Message = "Đã xảy ra lỗi khi quét mã." };
            }
        }

        public async Task SendPushNotification(Guid userId, int ticketId)
        {
            var user = await _unitOfWork.GetRepository<User>().GetByPropertyAsync(u => u.Id == userId);
            
            if (!string.IsNullOrEmpty(user.ExpoPushToken))
            {
                await SendExpoPushAsync(user.ExpoPushToken, "Quét vé thành công", $"Vé {ticketId} đã được quét", new { ticketId });
            }
        }

        public async Task SendExpoPushAsync(string expoPushToken, string title, string body, object? data = null)
        {
            var message = new
            {
                to = expoPushToken,
                title,
                body,
                data,
                sound = "default"
            };
            var payload = JsonSerializer.Serialize(message);

            using var http = new HttpClient { BaseAddress = new Uri("https://exp.host") };
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await http.PostAsync("/--/api/v2/push/send", content);

            var respBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Push failed: {respBody}");
            }
        }

        public async Task<TicketResponse?> CheckTicketExpire(int ticketId)
        {
            try
            {
                var ticket = await _ticketRepo.GetByPropertyAsync(t => t.Id == ticketId);
                if (ticket == null)
                {
                    return null;
                }

                if (ticket.ValidTo < DateTime.UtcNow)
                {
                    ticket.Status = TicketStatus.Expired;
                }

                await _ticketRepo.UpdateAsync(ticket);
                var succeedCount = await _unitOfWork.SaveAsync();
                if (succeedCount > 0)
                {
                    return new TicketResponse
                    {
                        TicketId = ticket.Id,
                        PassengerId = ticket.PassengerId,
                        PassengerName = ticket.Passenger.FirstName + " " + ticket.Passenger.LastName,
                        Email = ticket.Passenger.Email,
                        TicketTypeId = ticket.TicketTypeId,
                        TicketTypeName = ticket.TicketType.TicketTypeName,
                        TotalAmount = ticket.TotalAmount,
                        ValidTo = ticket.ValidTo,
                        PurchaseTime = ticket.PurchaseTime,
                        TrainRouteId = ticket.TrainRouteId ?? 0,
                        TrainRoutePrice = ticket.TrainRoute?.Price ?? 0m,
                        StartTerminal = ticket.TrainRoute?.StartTerminal ?? null,
                        EndTerminal = ticket.TrainRoute?.EndTerminal ?? null,
                        QRCode = ticket.QRCode,
                        Status = ticket.Status,
                        NumberOfTicket = ticket.NumberOfTicket,
                        IsPaid = ticket.isPaid,
                        TxnRef = ticket.TxnRef,
                        VnPayTransactionDate = ticket.VnPayTransactionDate,
                        VnPayTransactionNo = ticket.VnPayTransactionNo
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }
    }
}