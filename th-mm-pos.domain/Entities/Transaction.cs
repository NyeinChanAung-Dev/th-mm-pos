using th_mm_pos.domain.Enums;

namespace th_mm_pos.domain.Entities;

public class Transaction
{
    public int Id { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public int CashierId { get; set; }
    public User Cashier { get; set; } = null!;
    public bool IsVoided { get; set; }
    
    public ICollection<TransactionItem> Items { get; set; } = new List<TransactionItem>();
}
