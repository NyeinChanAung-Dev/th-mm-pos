using th_mm_pos.domain.Enums;

namespace th_mm_pos.application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedFulfillmentDate { get; set; }
    public OrderStatus Status { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByUsername { get; set; }
    public int? CompletedTransactionId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}