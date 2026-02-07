using th_mm_pos.domain.Enums;

namespace th_mm_pos.domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedFulfillmentDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public int CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;
    public int? CompletedTransactionId { get; set; }
    public Transaction? CompletedTransaction { get; set; }
    
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
