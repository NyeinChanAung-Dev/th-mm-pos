using th_mm_pos.domain.Enums;

namespace th_mm_pos.application.DTOs;

public class TransactionDto
{
    public int Id { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public int CashierId { get; set; }
    public string? CashierName { get; set; }
    public bool IsVoided { get; set; }
    public List<TransactionItemDto> Items { get; set; } = new();
}