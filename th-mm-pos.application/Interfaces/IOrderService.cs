using th_mm_pos.application.DTOs;
using th_mm_pos.domain.Enums;

namespace th_mm_pos.application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(OrderDto orderDto, int userId);
    Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, int userId);
    Task<OrderDto?> GetOrderByIdAsync(int orderId);
    Task<IEnumerable<OrderDto>> SearchOrdersAsync(OrderSearchDto searchDto);
    Task<bool> CancelOrderAsync(int orderId, int userId);
    Task<TransactionDto> CompleteOrderAsync(int orderId, int userId);
}