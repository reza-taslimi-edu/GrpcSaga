using Shared.Entities;

namespace ConsumerOne.Infrustructures.Repositories.Cities;

public interface ICityRepository
{
    void Add(CityEntity city);
    void Update(CityEntity city);
    void Delete(CityEntity city);
    List<CityEntity> GetAll();
    CityEntity? FindById(Guid cityId);
    CityEntity? FindByTitle(string title);
}