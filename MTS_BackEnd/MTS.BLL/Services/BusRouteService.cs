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
        private IGenericRepository<BusRoute> _busRouteRepo;

        public BusRouteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _busRouteRepo = _unitOfWork.GetRepository<BusRoute>();
        }

        public async Task<CreateBusRouteResponse> CreateBusRoute(CreateBusRouteRequest request)
        {
            try
            {
                // Tạo danh sách Terminal stub từ ID
                var terminals = request.TerminalId
                    .Select(id => new Terminal { Id = id })
                    .ToList();

                // Gán trạng thái Unchanged để EF hiểu là entity đã tồn tại
                foreach (var terminal in terminals)
                {
                    _unitOfWork.AttachAsUnchanged(terminal); // Cách tốt nhất nếu không expose DbContext
                }

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
                Console.WriteLine($"[CreateBusRoute ERROR] {ex}");
                return null;
            }
        }

        public async Task<bool> DeleteBusRoute(int id)
        {
            try
            {
                var br = await _busRouteRepo.GetByPropertyAsync(tr => tr.Id == id);
                if(br != null)
                {
                    br.IsDeleted = true;
                    await _busRouteRepo.UpdateAsync(br);
                    await _unitOfWork.SaveAsync();
                    return true;
                }   
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return false;
            }
        }

        public async Task<List<BusRouteDto>> GetAll()
        {
            try
            {
                var bus = await _busRouteRepo.GetAllByPropertyAsync(t => t.IsDeleted == false, includeProperties: "Terminals");
                if (!bus.Any())
                {
                    return new List<BusRouteDto>();
                }
                return bus.Select(b => new BusRouteDto
                {
                    BusRouteId = b.Id,
                    BusNumber = b.BusNumber,
                    TerminalId = b.Terminals.Select(t => t.Id).ToList(),
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }

        public async Task<BusRouteDto> GetById(int id)
        {
            try
            {
                var bus = await _busRouteRepo.GetByPropertyAsync(t => t.Id == id && t.IsDeleted == false, includeProperties: "Terminals");
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
                var model = await _busRouteRepo.GetByPropertyAsync(
                    t => t.Id == request.BusRouteId && t.IsDeleted == false
                );

                if (model == null)
                {
                    return new CreateBusRouteResponse { IsSuccess = false };
                }

                // Gán lại terminal theo danh sách ID
                var terminals = request.TerminalId
                    .Select(id => new Terminal { Id = id })
                    .ToList();

                foreach (var terminal in terminals)
                {
                    _unitOfWork.AttachAsUnchanged(terminal); // tránh bị hiểu nhầm là entity mới
                }

                // Cập nhật dữ liệu
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
                Console.WriteLine($"[UpdateBusRoute ERROR] {ex}");
                return null;
            }
        }
    }
}
