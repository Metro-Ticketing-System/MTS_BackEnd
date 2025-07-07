using MTS.DAL.Dtos;
using MTS.DAL.Repositories;
using MTS.Data.Models;

namespace MTS.BLL.Services
{
    public interface IBusRouteService
    {
        Task<BusRouteDto> GetById(int id);
        Task<CreateBusRouteResponse> CreateBusRoute(CreateBusRouteRequest request);
        Task<CreateBusRouteResponse> UpdateBusRoute(BusRouteDto request);
        Task<bool> DeleteBusRoute(int id);
    }

    public class BusRouteService : IBusRouteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private IGenericRepository<BusRoute> _busRouteRepo;
        private IGenericRepository<Terminal> _terminalRepo;

        public BusRouteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _busRouteRepo = _unitOfWork.GetRepository<BusRoute>();
            _terminalRepo = _unitOfWork.GetRepository<Terminal>();
        }

        public async Task<CreateBusRouteResponse> CreateBusRoute(CreateBusRouteRequest request)
        {
            try
            {
                var terminals = await _terminalRepo.GetAllByPropertyAsync(t => request.TerminalId.Contains(t.Id));
                var busRoute = new BusRoute
                {
                    CreatedTime = DateTime.Now,
                    CreatedBy = request.PassengerId.ToString(),
                    LastUpdatedTime = DateTime.Now,

                    BusNumber = request.BusNumber,
                    Terminals = terminals
                };
                await _busRouteRepo.AddAsync(busRoute);
                var succeedCount = await _unitOfWork.SaveAsync();
                if (succeedCount > 0)
                {
                    return new CreateBusRouteResponse
                    {
                        IsSuccess = true,
                        Id = busRoute.Id
                    };
                }
                return new CreateBusRouteResponse { IsSuccess = false };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteBusRoute(int id)
        {
            try
            {
                await _busRouteRepo.DeleteAsync(id);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return false;
            }
        }

        public async Task<BusRouteDto> GetById(int id)
        {
            try
            {
                var bus = await _busRouteRepo.GetByPropertyAsync(t => t.Id == id, includeProperties: "Terminals");
                if (bus == null)
                {
                    return null;
                }
                var result = new BusRouteDto
                {
                    BusRouteId = bus.Id,
                    BusNumber = bus.BusNumber,
                    TerminalId = bus.Terminals.Select(t => t.Id).ToList(),
                };
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }

        public async Task<CreateBusRouteResponse> UpdateBusRoute(BusRouteDto request)
        {
            try
            {
                var terminals = await _terminalRepo.GetAllByPropertyAsync(t => request.TerminalId.Contains(t.Id));
                var model = await _busRouteRepo.GetByPropertyAsync(t => t.Id == request.BusRouteId);
                if(model == null)
                {
                    return new CreateBusRouteResponse { IsSuccess = false };
                }
                model.BusNumber = request.BusNumber;
                model.Terminals = terminals;
                await _busRouteRepo.UpdateAsync(model);
                var succeedCount = await _unitOfWork.SaveAsync();
                if (succeedCount > 0)
                {
                    return new CreateBusRouteResponse
                    {
                        IsSuccess = true,
                        Id = model.Id
                    };
                }
                return new CreateBusRouteResponse { IsSuccess = false };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }
    }
}
