namespace Shared.Entities
{
    public class CityEntity
    {
        public CityEntity(Guid id, string? name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; private set; }
        public string? Name { get; private set; }
        public bool Deleted { get; private set; }

        public void Update(string name, bool deleted)
        {
            Name = name;
            Deleted = deleted;
        }
        public void ChangeDeleted()
        {
            Deleted = !Deleted;
        }
    }
}
