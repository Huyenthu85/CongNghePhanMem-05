
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data.Models;
namespace ThuyetMinh.Data.Repositories;
public interface IPaymentOrderRepository
{
    Task<PaymentOrder> CreateAsync(PaymentOrder order);
    Task<PaymentOrder?> GetByOrderCodeAsync(string orderCode);
}
public class PaymentOrderRepository : IPaymentOrderRepository
{
    private readonly AppDbContext _db;
    public PaymentOrderRepository(AppDbContext db)
    {
        _db = db;
    }
    public async Task<PaymentOrder> CreateAsync(PaymentOrder order)
    {
        _db.PaymentOrders.Add(order);
        await _db.SaveChangesAsync();
        return order;
    }
    public async Task<PaymentOrder?> GetByOrderCodeAsync(string orderCode)
    {
        return await _db.PaymentOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrderCode == orderCode);
    }
}
