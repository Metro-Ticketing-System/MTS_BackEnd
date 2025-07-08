// MTS.BLL/Services/TicketService.cs
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
		Task<TicketDto?> GetTicketById(int id);
		Task<bool> DeleteTicket(int id);
		Task<CreateTicketResponseDto> UpdateTicket(TicketDto ticket);
	}

	public class TicketService : ITicketService
	{
		private readonly IUnitOfWork _unitOfWork;
		private IGenericRepository<Ticket> _ticketRepo;

		public TicketService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
			_ticketRepo = _unitOfWork.GetRepository<Ticket>();
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

		public async Task<TicketDto?> GetTicketById(int id)
		{
			try
			{
				var ticket = await _ticketRepo.GetByPropertyAsync(t => t.Id == id & t.IsDeleted == false, includeProperties: "Passenger");
				if (ticket == null)
				{
					return null;
				}
				var dto = new TicketDto
				{
					TicketId = ticket.Id,
					PassengerId = ticket.PassengerId,
					TicketTypeId = ticket.TicketTypeId,
					TotalAmount = ticket.TotalAmount,
					ValidTo = ticket.ValidTo,
					PurchaseTime = ticket.PurchaseTime,
					TrainRouteId = ticket.TrainRouteId,
					QRCode = ticket.QRCode,
					Status = ticket.Status,
					NumberOfTicket = ticket.NumberOfTicket,
					PassengerName = ticket.Passenger.FirstName + " " + ticket.Passenger.LastName,
					isPaid = ticket.isPaid,
					TxnRef = ticket.TxnRef,
					VnPayTransactionNo = ticket.VnPayTransactionNo,
					VnPayTransactionDate = ticket.VnPayTransactionDate
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
	}
}