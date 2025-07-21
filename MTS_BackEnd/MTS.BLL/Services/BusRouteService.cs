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
        Task<List<BusRouteDto>> GetAll();
    }

    public class BusRouteService : IBusRouteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<BusRoute> _busRouteRepo;

        public BusRouteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _busRouteRepo = _unitOfWork.GetRepository<BusRoute>();
        }

        public async Task<CreateBusRouteResponse> CreateBusRoute(CreateBusRouteRequest request)
        {
            try
            {
                var busRoute = new BusRoute
                {
                    CreatedTime = DateTime.Now,
                    CreatedBy = request.PassengerId.ToString(),
                    LastUpdatedTime = DateTime.Now,
                    BusNumber = request.BusNumber,
                    BusRouteTerminals = request.TerminalId.Select(tid => new BusRouteTerminal
                    {
                        TerminalId = tid
                    }).ToList()
                };

                await _busRouteRepo.AddAsync(busRoute);
                var result = await _unitOfWork.SaveAsync();

                return new CreateBusRouteResponse
                {
                    IsSuccess = result > 0,
                    Id = busRoute.Id
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateBusRoute ERROR] {ex}");
                return null;
            }
        }

        public async Task<CreateBusRouteResponse> UpdateBusRoute(BusRouteDto request)
        {
            try
            {
                var busRoute = await _busRouteRepo.GetByPropertyAsync(
                    t => t.Id == request.BusRouteId && t.IsDeleted == false,
                    includeProperties: "BusRouteTerminals"
                );

                if (busRoute == null)
                {
                    return new CreateBusRouteResponse { IsSuccess = false };
                }

                busRoute.BusNumber = request.BusNumber;
                busRoute.LastUpdatedTime = DateTime.Now;

                // Lấy repository trung gian
                var busRouteTerminalRepo = _unitOfWork.GetRepository<BusRouteTerminal>();

                // Xóa từng liên kết cũ
                foreach (var brt in busRoute.BusRouteTerminals.ToList())
                {
                    await busRouteTerminalRepo.DeleteAsync(brt.BusRouteId, brt.TerminalId);
                }

                // Thêm lại các liên kết mới
                var newLinks = request.TerminalId.Select(id => new BusRouteTerminal
                {
                    BusRouteId = request.BusRouteId,
                    TerminalId = id
                });

                await busRouteTerminalRepo.AddRangeAsync(newLinks);

                var result = await _unitOfWork.SaveAsync();

                return new CreateBusRouteResponse
                {
                    IsSuccess = result > 0,
                    Id = busRoute.Id
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateBusRoute ERROR] {ex}");
                return null;
            }
        }


        public async Task<bool> DeleteBusRoute(int id)
        {
            try
            {
                var busRoute = await _busRouteRepo.GetByPropertyAsync(t => t.Id == id);
                if (busRoute != null)
                {
                    busRoute.IsDeleted = true;
                    await _busRouteRepo.UpdateAsync(busRoute);
                    await _unitOfWork.SaveAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteBusRoute ERROR] {ex.Message}");
                return false;
            }
        }

        public async Task<BusRouteDto> GetById(int id)
        {
            try
            {
                var busRoute = await _busRouteRepo.GetByPropertyAsync(
                    t => t.Id == id && t.IsDeleted == false,
                    includeProperties: "BusRouteTerminals.Terminal"
                );

                if (busRoute == null)
                    return null;

                return new BusRouteDto
                {
                    BusRouteId = busRoute.Id,
                    BusNumber = busRoute.BusNumber,
                    TerminalId = busRoute.BusRouteTerminals
                                    .Select(brt => brt.TerminalId)
                                    .ToList()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetById ERROR] {ex}");
                return null;
            }
        }

        public async Task<List<BusRouteDto>> GetAll()
        {
            try
            {
                var busRoutes = await _busRouteRepo.GetAllByPropertyAsync(
                    t => !t.IsDeleted,
                    includeProperties: "BusRouteTerminals.Terminal"
                );

                return busRoutes.Select(b => new BusRouteDto
                {
                    BusRouteId = b.Id,
                    BusNumber = b.BusNumber,
                    TerminalId = b.BusRouteTerminals.Select(brt => brt.TerminalId).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAll ERROR] {ex}");
                return new List<BusRouteDto>();
            }
        }
    }
}
