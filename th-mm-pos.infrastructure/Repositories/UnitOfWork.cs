using Microsoft.EntityFrameworkCore.Storage;
using th_mm_pos.domain.Entities;
using th_mm_pos.domain.Interfaces;
using th_mm_pos.infrastructure.Data;

namespace th_mm_pos.infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public IRepository<User> Users => field ??= new Repository<User>(context);
    public IRepository<Product> Products => field ??= new Repository<Product>(context);
    public IRepository<Transaction> Transactions => field ??= new Repository<Transaction>(context);
    public IRepository<Order> Orders => field ??= new Repository<Order>(context);

    public IRepository<TransactionItem> TransactionItems =>
        field ??= new Repository<TransactionItem>(context);

    public IRepository<Role> Roles => field ??= new Repository<Role>(context);
    public IRepository<Permission> Permissions => field ??= new Repository<Permission>(context);
    public IRepository<AuditLog> AuditLogs => field ??= new Repository<AuditLog>(context);

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
    }
}