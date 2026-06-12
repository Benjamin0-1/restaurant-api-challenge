namespace Restaurant.Requests;

public class ProductCreateRequest
{
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public decimal Price { get; set; }
    
    public string Sku { get; set; }
    
    public bool IsAvailable { get; set; }
    
    public int CategoryId { get; set; }
    
    public int USerId { get; set; } // se extrae de los claims y por lo tanto el usuario no lo puede asignar.
}
