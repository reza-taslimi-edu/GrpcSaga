using ConsumerTwo.Infrustructures.Repositories.Cities;
using Grpc.Core;
using Newtonsoft.Json;
using protoServices.Grpc;
using Shared.Entities;
using Shared.Extensions;

namespace ConsumerTwo.ProtosServices
{
    public class GrpcCityService : CityService.CityServiceBase
    {
        private readonly ILogger<GrpcCityService> _logger;
        private readonly ICityRepository _cityRepository;

        public GrpcCityService(ILogger<GrpcCityService> logger, ICityRepository cityRepository)
        {
            _logger = logger;

            _cityRepository = cityRepository;
        }

        public override async Task<CityResponse> AddCity(CityRequest request, ServerCallContext context)
        {
            CityEntity city = new CityEntity(request.Id.ToGuid(), request.Name);

            _cityRepository.Add(city);

            return new CityResponse
            {
                Success = true,
                Message = "City added successfully",
                Object = JsonConvert.SerializeObject(city)
            };
        }

        public override async Task<CityResponse> UpdateCity(CityRequest request, ServerCallContext context)
        {
            var city = _cityRepository.FindById(request.Id.ToGuid());

            if (request.Name.Length < 5)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Minimum length is five characters."));
            }

            city.Update(city.Name, request.Deleted);

            _cityRepository.Update(city);

            return new CityResponse
            {
                Success = true,
                Message = "City update successfully",
                Object = JsonConvert.SerializeObject(city)
            };
        }

        public override async Task<CityResponse> DeleteCity(DeleteRequest request, ServerCallContext context)
        {
            var city = _cityRepository.FindById(request.Id.ToGuid());

            city.Update(city.Name, true);

            _cityRepository.Update(city);

            return new CityResponse
            {
                Success = true,
                Message = "City deleted successfully",
            };
        }

        public override async Task<CityResponse> RollbackToAddCity(CityRequest request, ServerCallContext context)
        {
            var city = _cityRepository.FindById(request.Id.ToGuid());

            city.Update(city.Name, true);

            _cityRepository.Update(city);

            return new CityResponse
            {
                Success = true,
                Message = "City rollback to add successfully",
            };
        }

        public override async Task<CityResponse> RollbackToUpdateCity(CityRequest request, ServerCallContext context)
        {
            var city = _cityRepository.FindById(request.Id.ToGuid());

            city.Update(request.Name, city.Deleted);

            _cityRepository.Update(city);

            return new CityResponse
            {
                Success = true,
                Message = "City rollback to update successfully",
            };
        }

        public override async Task<CityResponse> RollbackToDeleteCity(DeleteRequest request, ServerCallContext context)
        {
            var city = _cityRepository.FindById(request.Id.ToGuid());

            city.Update(city.Name, false);

            _cityRepository.Update(city);

            return new CityResponse
            {
                Success = true,
                Message = "City rollback to delete successfully",
            };
        }
    }
}