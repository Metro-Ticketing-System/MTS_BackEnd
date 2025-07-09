using MTS.DAL.Dtos;
using MTS.DAL.Repositories;
using MTS.Data.Models;

namespace MTS.BLL.Services
{
    public interface ITicketTypeService
    {
        Task<List<TicketTypeDto>> GetListTicketType();
    }
    public class TicketTypeService : ITicketTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private IGenericRepository<TicketType> _ticketTypeRepo;
        public TicketTypeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ticketTypeRepo = _unitOfWork.GetRepository<TicketType>();
        }
        public async Task<List<TicketTypeDto>> GetListTicketType()
        {
            try
            {
                var tt = await _ticketTypeRepo.GetAllByPropertyAsync();
                var result = tt.Select(t => new TicketTypeDto
                {
                    TicketTypeName = t.TicketTypeName,
                    Price = t.Price,
                }).ToList();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }
    }
}
