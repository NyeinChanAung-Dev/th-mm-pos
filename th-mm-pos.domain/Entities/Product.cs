namespace th_mm_pos.domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    
    public ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
