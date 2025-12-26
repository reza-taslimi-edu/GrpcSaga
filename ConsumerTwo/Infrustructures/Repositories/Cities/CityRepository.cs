using ConsumerTwo.Infrustructures;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace ConsumerTwo.Infrustructures.Repositories.Cities;

public class CityRepository(ConsumerTwoDbContext dbContext) : ICityRepository
{
    public void Add(CityEntity city)
    {
        dbContext.Cities.Add(city);
        dbContext.SaveChanges();
    }

    public void Delete(CityEntity city)
    {
        var found = dbContext.Cities.AsNoTracking().FirstOrDefault(x => x.Id == city.Id);
        if (found != null)
        {
            found.ChangeDeleted();
            dbContext.Cities.Update(found);
            dbContext.SaveChanges();
        }
    }

    public CityEntity? FindById(Guid cityId)
    {
        return dbContext.Cities.FirstOrDefault(x => x.Id == cityId);
    }

    public CityEntity? FindByTitle(string name)
    {
        return dbContext.Cities.FirstOrDefault(x => x.Name == name);
    }

    public List<CityEntity> GetAll()
    {
        return dbContext.Cities.ToList();
    }

    public void Update(CityEntity city)
    {
        dbContext.Cities.Update(city);
        dbContext.SaveChanges();
    }
}