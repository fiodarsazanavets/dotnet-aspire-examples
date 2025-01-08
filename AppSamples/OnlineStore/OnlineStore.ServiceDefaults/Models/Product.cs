namespace OnlineStore.ServiceDefaults.Models;

public class Product
{
    public Product(string name, decimal price, string description)
    {
        Name = name;
        Price = price;
        Description = description;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}
    
    
    
    
;