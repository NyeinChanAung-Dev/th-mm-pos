namespace th_mm_pos.application.DTOs;

public class ReceiptDto
{
    public int TransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public List<TransactionItemDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}