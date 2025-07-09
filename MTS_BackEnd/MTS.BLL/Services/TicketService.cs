// MTS.BLL/Services/TicketService.cs
using MTS.BLL.Services.QRService;
using MTS.DAL.Dtos;
using MTS.DAL.Repositories;
using MTS.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
					TrainRouteId = createTicketRequestDto.TrainRouteId,
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
                    TrainRouteId = ticket.TrainRouteId,
                    TrainRoutePrice = ticket.TrainRoute?.Price,
                    StartTerminal = ticket.TrainRoute?.StartTerminal,
                    EndTerminal = ticket.TrainRoute?.EndTerminal,
                    QRCode = ticket.QRCode,
                    Status = ticket.Status,
                    NumberOfTicket = ticket.NumberOfTicket,
                    isPaid = ticket.isPaid,
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
					TrainRouteId = t.TrainRouteId,
					TrainRoutePrice = t.TrainRoute.Price,
					StartTerminal = t.TrainRoute.StartTerminal,
					EndTerminal = t.TrainRoute.EndTerminal,
					QRCode = t.QRCode,
					Status = t.Status,
					NumberOfTicket = t.NumberOfTicket,
					isPaid = t.isPaid,
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
    }
}