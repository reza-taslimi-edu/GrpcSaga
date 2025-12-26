using ConsumerOne.Infrustructures.Repositories.Cities;
using Microsoft.AspNetCore.Mvc;
using Shared.Entities;

namespace ConsumerOne.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CityController : ControllerBase
    {

        private readonly ICityRepository _cityRepository;

        public CityController(ICityRepository cityRepository)
        {
            _cityRepository = cityRepository;
        }

        [HttpGet]
        public IEnumerable<CityEntity> GetAll()
        {
            return _cityRepository.GetAll();
        }
    }
}
