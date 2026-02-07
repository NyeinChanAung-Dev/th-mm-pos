using th_mm_pos.application.DTOs;

namespace th_mm_pos.application.Interfaces;

public interface ISalesService
{
    Task<TransactionDto> CreateTransactionAsync(TransactionDto transactionDto, int cashierId);
    Task<TransactionDto> VoidTransactionAsync(int transactionId, int userId);
    Task<TransactionDto> ProcessReturnAsync(int originalTransactionId, List<int> itemIds, int userId);
    Task<decimal> CalculateTotalAsync(List<TransactionItemDto> items, decimal discount);
    Task<byte[]> GenerateReceiptAsync(int transactionId);
}