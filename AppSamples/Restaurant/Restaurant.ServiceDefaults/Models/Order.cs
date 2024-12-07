using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.ServiceDefaults.Models;

public class Order
{
    [Key]
    public int OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;

    [NotMapped]
    public decimal OrderTotal => Items.Sum(i => i.Item.Price * i.Quantity);

    ICollection<OrderItem> Items { get; set; } = [];
}
