using MTS.DAL.Dtos;
using MTS.DAL.Repositories;
using MTS.Data.Models;

namespace MTS.BLL.Services
{
    public interface ITrainRouteService
    {
        Task<GetTrainRouteResponse> GetTrainRoute(GetTrainRouteRequest request);
        Task<CreateTrainRouteResponse> CreateTrainRoute(CreateTrainRouteRequest request);
        Task<CreateTrainRouteResponse> UpdateTrainRoute(TrainRouteDto request);
        Task<bool> DeleteTrainRoute(int id);
    }
    public class TrainRouteService : ITrainRouteService
    {

        private IGenericRepository<TrainRoute> _trainRouteRepo;
        private IGenericRepository<Terminal> _terminalRepo;
        private readonly IUnitOfWork _unitOfWork;
        public TrainRouteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _trainRouteRepo = _unitOfWork.GetRepository<TrainRoute>();
            _terminalRepo = _unitOfWork.GetRepository<Terminal>();
        }
        public async Task<GetTrainRouteResponse> GetTrainRoute(GetTrainRouteRequest request)
        {
            try
            {
                var route = await _trainRouteRepo.GetByPropertyAsync(t => t.StartTerminal == request.StartTerminal && t.EndTerminal == request.EndTerminal);
                if(route == null)
                {
                    Console.WriteLine();
                    return new GetTrainRouteResponse();
                }    
                var result = new GetTrainRouteResponse() 
                { 
                    TrainRouteId = route.Id,
                    Price = route.Price,
                    StartTerminal = route.StartTerminal,
                    EndTerminal = route.EndTerminal,
                };
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }

        public async Task<CreateTrainRouteResponse> CreateTrainRoute(CreateTrainRouteRequest request)
        {
            try
            {
                var start = _terminalRepo.GetByPropertyAsync(t => t.Id == request.StartTerminal);
                var end = _terminalRepo.GetByPropertyAsync(t => t.Id == request.EndTerminal);
                if(start == null || end == null)
                {
                    return new CreateTrainRouteResponse { IsSuccess = false};
                }

                var trainRoute = new TrainRoute
                {
                    CreatedTime = DateTime.Now,
                    CreatedBy = request.PassengerId.ToString(),
                    LastUpdatedTime = DateTime.Now,

                    Price = request.Price,
                    StartTerminal = request.StartTerminal,
                    EndTerminal = request.EndTerminal,
                };
                await _trainRouteRepo.AddAsync(trainRoute);
                var succeedCount = await _unitOfWork.SaveAsync();
                if (succeedCount > 0)
                {
                    return new CreateTrainRouteResponse
                    {
                        IsSuccess = true,
                        Id = trainRoute.Id
                    };
                }
                return new CreateTrainRouteResponse { IsSuccess = false };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }

        public async Task<CreateTrainRouteResponse> UpdateTrainRoute(TrainRouteDto request)
        {
            try
            {
                var model = await _trainRouteRepo.GetByPropertyAsync(t => t.Id == request.Id);
                if (model == null)
                {
                    return new CreateTrainRouteResponse { IsSuccess = false };
                }

                var start = _terminalRepo.GetByPropertyAsync(t => t.Id == request.StartTerminal);
                var end = _terminalRepo.GetByPropertyAsync(t => t.Id == request.EndTerminal);
                if (start == null || end == null)
                {
                    return new CreateTrainRouteResponse { IsSuccess = false };
                }

                model.StartTerminal = request.StartTerminal;
                model.EndTerminal = request.EndTerminal;
                model.Price = request.Price;
                await _trainRouteRepo.UpdateAsync(model);
                var succeedCount = await _unitOfWork.SaveAsync();
                if (succeedCount > 0)
                {
                    return new CreateTrainRouteResponse
                    {
                        IsSuccess = true,
                        Id = model.Id
                    };
                }
                return new CreateTrainRouteResponse { IsSuccess = false };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteTrainRoute(int id)
        {
            try
            {
                var tr = await _trainRouteRepo.GetByPropertyAsync(tr => tr.Id == id);
                tr.IsDeleted = true;
                await _trainRouteRepo.UpdateAsync(tr);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return false;
            }
        }
    }
}
