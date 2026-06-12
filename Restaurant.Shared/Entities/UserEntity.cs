namespace Restaurant.Shared.Entities;

public class UserEntity : EntityBase
{
    public string Name { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }
    
    public ICollection<ProductEntity> Products { get; set; }
}
