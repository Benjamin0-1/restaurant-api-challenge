namespace Restaurant.Shared.Entities;

public class EntityBase
{
    public int Id { get; set; }
    
    public bool IsDeleted { get; set; } =  false;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}
