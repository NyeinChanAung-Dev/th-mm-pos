using th_mm_pos.domain.Enums;

namespace th_mm_pos.application.DTOs;

public class OrderSearchDto
{
    public OrderStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CustomerName { get; set; }
}