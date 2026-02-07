using th_mm_pos.domain.Entities;

namespace th_mm_pos.domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Product> Products { get; }
    IRepository<Transaction> Transactions { get; }
    IRepository<Order> Orders { get; }
    IRepository<TransactionItem> TransactionItems { get; }
    IRepository<Role> Roles { get; }
    IRepository<Permission> Permissions { get; }
    IRepository<AuditLog> AuditLogs { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
