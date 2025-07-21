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
        Task<TerminalDto> CreateTerminal(CreateTerminalRequest request);
    }

    public class TerminalService : ITerminalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Terminal> _terminalRepo;

        public TerminalService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _terminalRepo = _unitOfWork.GetRepository<Terminal>();
        }

        public async Task<TerminalDto> CreateTerminal(CreateTerminalRequest request)
        {
            try
            {
                var terminal = new Terminal
                {
                    Name = request.Name,
                    Location = request.Location,
                    CreatedBy = request.UserId.ToString(),
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                };

                await _terminalRepo.AddAsync(terminal);
                var succeedCount = await _unitOfWork.SaveAsync();

                if (succeedCount > 0)
                {
                    return new TerminalDto
                    {
                        Id = terminal.Id,
                        Name = terminal.Name,
                        Location = terminal.Location
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateTerminal ERROR] {ex.Message}");
                return null;
            }
        }

        public async Task<List<GetAllTerminalsResponse>> GetAllTerminals()
        {
            try
            {
                var terminals = await _terminalRepo.GetAllByPropertyAsync(t => t.IsDeleted == false);
                if (terminals == null || terminals.Count == 0)
                {
                    return new List<GetAllTerminalsResponse>();
                }

                return terminals.Select(t => new GetAllTerminalsResponse
                {
                    Id = t.Id,
                    Name = t.Name,
                    Location = t.Location
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAllTerminals ERROR] {ex.Message}");
                return null;
            }
        }

        public async Task<List<GetBusRoutesResponse>> GetBusRoutes(int terminalId)
        {
            try
            {
                var terminal = await _terminalRepo.GetByPropertyAsync(
                    t => t.Id == terminalId && t.IsDeleted == false,
                    includeProperties: "BusRouteTerminals.BusRoute"
                );

                if (terminal == null)
                {
                    return new List<GetBusRoutesResponse>();
                }

                var result = terminal.BusRouteTerminals
                    .Select(brt => brt.BusRoute)
                    .Where(b => b != null && !b.IsDeleted)
                    .Select(b => new GetBusRoutesResponse
                    {
                        Id = b.Id,
                        BusNumber = b.BusNumber
                    })
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetBusRoutes ERROR] {ex.Message}");
                return null;
            }
        }

        public async Task<TerminalDto> GetTerminalById(int terminalId)
        {
            try
            {
                var terminal = await _terminalRepo.GetByPropertyAsync(
                    t => t.Id == terminalId && t.IsDeleted == false,
                    includeProperties: "StartRoutes,EndRoutes,BusRouteTerminals.BusRoute"
                );

                if (terminal == null)
                {
                    return null;
                }

                var busRoutes = terminal.BusRouteTerminals
                    .Select(brt => brt.BusRoute)
                    .Where(b => b != null && !b.IsDeleted)
                    .ToList();

                return new TerminalDto
                {
                    Id = terminal.Id,
                    Name = terminal.Name,
                    Location = terminal.Location,
                    StartRoutes = terminal.StartRoutes?.ToList() ?? new List<TrainRoute>(),
                    EndRoutes = terminal.EndRoutes?.ToList() ?? new List<TrainRoute>(),
                    BusRoutes = busRoutes
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetTerminalById ERROR] {ex.Message}");
                return null;
            }
        }
    }
}
