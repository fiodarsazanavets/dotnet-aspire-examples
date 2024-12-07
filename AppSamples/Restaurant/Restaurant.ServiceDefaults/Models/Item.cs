using System.ComponentModel.DataAnnotations;

namespace Restaurant.ServiceDefaults.Models;

public class Item
{
    [Key]
    public int ItemId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}
