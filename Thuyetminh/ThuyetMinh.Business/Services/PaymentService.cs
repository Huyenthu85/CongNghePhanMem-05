
using System.Security.Cryptography;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;
namespace ThuyetMinh.Business.Services;
public interface IPaymentService
{
    Task<PaymentOrder> CreateOrderAsync(long anonymousVisitorId);
    Task<PaymentOrder?> GetOrderAsync(string orderCode);
}
public class PaymentService : IPaymentService
{
    private const long AmountVnd = 20000;
    private const long GrantedSeconds = 900;
    private readonly IPaymentOrderRepository _repository;
    public PaymentService(IPaymentOrderRepository repository)
    {
        _repository = repository;
    }
    public async Task<PaymentOrder> CreateOrderAsync(long anonymousVisitorId)
    {
        if (anonymousVisitorId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(anonymousVisitorId));
        }
        var order = new PaymentOrder
        {
            OrderCode = "TM" + Convert.ToHexString(RandomNumberGenerator.GetBytes(12)),
            AnonymousVisitorId = anonymousVisitorId,
            AmountVnd = AmountVnd,
            GrantedSeconds = GrantedSeconds,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };
        return await _repository.CreateAsync(order);
    }
    public Task<PaymentOrder?> GetOrderAsync(string orderCode)
    {
        return _repository.GetByOrderCodeAsync(orderCode);
    }
}
