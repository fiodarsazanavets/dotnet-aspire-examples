using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.ServiceDefaults.Models;

public class OrderItem
{
    public int OrderId { get; set; }
    public int ItemId { get; set; }

    public ushort Quantity { get; set; } = 1;

    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item Item { get; set; }
}
