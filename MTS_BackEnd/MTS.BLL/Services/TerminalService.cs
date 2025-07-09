using MTS.DAL.Dtos;
using MTS.DAL.Repositories;
using MTS.Data.Models;

namespace MTS.BLL.Services
{
    public interface ITerminalService
    {
        Task<List<GetBusRoutesResponse>> GetBusRoutes(int terminalId);
        Task<List<GetAllTerminalsResponse>> GetAllTerminals();
        Task<TerminalDto> GetTerminalById(int terminalId);
    }
    public class TerminalService : ITerminalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private IGenericRepository<Terminal> _terminalRepo;

        public TerminalService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _terminalRepo = _unitOfWork.GetRepository<Terminal>();
        }
        public async Task<List<GetAllTerminalsResponse>> GetAllTerminals()
        {
            try
            {
                var terminals = await _terminalRepo.GetAllByPropertyAsync(t => t.IsDeleted == false);
                if(terminals == null || terminals.Count == 0)
                {
                    Console.WriteLine("No terminal found.");
                    return new List<GetAllTerminalsResponse>();
                }

                var result = terminals.Select(t => new GetAllTerminalsResponse
                {
                    Id = t.Id,
                    Name = t.Name,
                    Location = t.Location,
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }

        public async Task<List<GetBusRoutesResponse>> GetBusRoutes(int terminalId)
        {
            try
            {
                var terminal = await _terminalRepo.GetByPropertyAsync(t => t.Id == terminalId && t.IsDeleted == false, includeProperties: "BusRoutes");
                if(terminal == null)
                {
                    Console.WriteLine("No terminal found.");
                    return new List<GetBusRoutesResponse>();
                }
                var result = terminal.BusRoutes.Select(t => new GetBusRoutesResponse
                { 
                    Id = t.Id,
                    BusNumber = t.BusNumber 
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }

        public async Task<TerminalDto> GetTerminalById(int terminalId)
        {
            try
            {
                var terminal = await _terminalRepo.GetByPropertyAsync(t => t.Id == terminalId && t.IsDeleted == false, includeProperties: "StartRoutes, EndRoutes, BusRoutes");
                if (terminal == null) 
                {
                    return null;
                }
                var result = new TerminalDto 
                { 
                    Id = terminalId,
                    Name = terminal.Name,
                    Location = terminal.Location,
                    StartRoutes = terminal.StartRoutes.ToList(),
                    EndRoutes = terminal.EndRoutes.ToList(),
                    BusRoutes = terminal.BusRoutes.ToList(),
                };
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
