namespace Restaurant.Shared.Entities;

public class ProductEntity : EntityBase
{
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public decimal Price { get; set; }
    
    public string Sku { get; set; }
    
    public bool IsAvailable { get; set; }
    
    public int CategoryId { get; set; }

    public CategoryEntity Category { get; set; }

    public int UserId { get; set; }

    public UserEntity User { get; set; }
}

