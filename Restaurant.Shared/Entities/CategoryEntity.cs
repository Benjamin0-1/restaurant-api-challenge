namespace Restaurant.Shared.Entities;

public class CategoryEntity : EntityBase
{
    public string Name { get; set; }

    public string Description { get; set; }

    public bool IsActive { get; set; }

    public ICollection<ProductEntity> Products { get; set; }
}

