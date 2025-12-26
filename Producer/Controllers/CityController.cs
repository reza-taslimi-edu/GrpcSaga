using Microsoft.AspNetCore.Mvc;
using Producer.Infrustructures.Repositories.Cities;
using Producer.ProtosServices;
using protoServices.Grpc;
using Shared.Entities;

namespace Producer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CityController : ControllerBase
    {
        private readonly ICityRepository _cityRepository;
        private readonly GrpcCityServiceClient _grpcCityService;

        public CityController(ICityRepository cityRepository, GrpcCityServiceClient grpcCityService)
        {
            _cityRepository = cityRepository;
            _grpcCityService = grpcCityService;
        }

        [HttpGet]
        public IEnumerable<CityEntity> GetAll()
        {
            return _cityRepository.GetAll();
        }

        [HttpPost]
        public async Task<ActionResult<CityEntity>> AddAsync(string title)
        {
            CityEntity city = new CityEntity(Guid.NewGuid(), title);

            var cityRequest = new CityRequest { Id = city.Id.ToString(), Name = city.Name };

            try
            {
                var result = await _grpcCityService.AddCityAsync(cityRequest);

                if (result.Success)
                {
                    _cityRepository.Add(city);
                }
                else
                {
                    return BadRequest(result.Message);
                }

                return city;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch]
        public async Task<ActionResult<CityEntity>> UpdateAsync(Guid id, string title)
        {
            var city = _cityRepository.FindById(id);

            var cityRequest = new CityRequest { Id = city.Id.ToString(), Name = city.Name };

            try
            {
                city.Update(title, city.Deleted);

                var result = await _grpcCityService.UpdateCityAsync(cityRequest);

                if (result.Success)
                {
                    _cityRepository.Update(city);
                }
                else
                {
                    return BadRequest(result.Message);
                }

                return city;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteAsync(Guid id)
        {
            var city = _cityRepository.FindById(id);

            var cityRequest = new DeleteRequest { Id = city.Id.ToString() };

            try
            {
                var result = await _grpcCityService.DeleteCityAsync(cityRequest);

                if (result.Success)
                {
                    _cityRepository.Delete(city);
                }
                else
                {
                    return BadRequest(result.Message);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
