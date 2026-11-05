using System.Security.Cryptography;

namespace ThuyetMinh.Api.Services;

/// <summary>
/// Temporary DEMO QR payment session. One QR maps to exactly one existing
/// PaymentOrder owned by the browser that opened the checkout page.
/// No bank transaction is involved. Restarting the app invalidates these codes.
/// </summary>
public sealed class DemoQrTokenStore
{
    private sealed class Ticket
    {
        public required long VisitorId { get; init; }
        public required string OrderCode { get; init; }
        public required DateTimeOffset ExpiresAt { get; init; }
        public bool Processing { get; set; }
        public bool Completed { get; set; }
    }

    public enum RedeemState { Invalid, Busy, AlreadyCompleted, Reserved }
    public readonly record struct Redemption(RedeemState State, string? OrderCode, long VisitorId);

    private readonly Dictionary<string, Ticket> _tickets = new(StringComparer.Ordinal);
    private readonly object _lock = new();
    private readonly TimeProvider _clock;
    public DemoQrTokenStore() : this(TimeProvider.System) { }
    public DemoQrTokenStore(TimeProvider clock) => _clock = clock;

    public (string Code, DateTimeOffset ExpiresAt) Issue(long visitorId, string orderCode, DateTimeOffset orderExpiry)
    {
        if (visitorId <= 0 || string.IsNullOrWhiteSpace(orderCode))
            throw new ArgumentException("Invalid demo payment order.");
        lock (_lock)
        {
            var now = _clock.GetUtcNow();
            foreach (var key in _tickets.Where(t => t.Value.ExpiresAt <= now)
                                        .Select(t => t.Key).ToArray()) _tickets.Remove(key);
            if (orderExpiry <= now) throw new InvalidOperationException("Đơn hàng đã hết hạn.");
            if (_tickets.Count >= 20)
                throw new InvalidOperationException("Đã có quá nhiều QR thử nghiệm đang hoạt động.");
            var expiresAt = now.AddMinutes(5);
            if (orderExpiry < expiresAt) expiresAt = orderExpiry;
            var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
            _tickets.Add(code, new Ticket { VisitorId = visitorId,
                OrderCode = orderCode, ExpiresAt = expiresAt });
            return (code, expiresAt);
        }
    }

    public Redemption Begin(string? code)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(code) || code.Length != 48 || !code.All(Uri.IsHexDigit) ||
                !_tickets.TryGetValue(code, out var ticket))
                return new(RedeemState.Invalid, null, 0);
            // A successful result remains recognizable until the QR expires.
            if (ticket.Completed)
                return new(RedeemState.AlreadyCompleted, ticket.OrderCode, ticket.VisitorId);
            if (ticket.ExpiresAt <= _clock.GetUtcNow())
            {
                _tickets.Remove(code);
                return new(RedeemState.Invalid, null, 0);
            }
            if (ticket.Processing)
                return new(RedeemState.Busy, ticket.OrderCode, ticket.VisitorId);
            ticket.Processing = true;
            return new(RedeemState.Reserved, ticket.OrderCode, ticket.VisitorId);
        }
    }

    public void Finish(string? code, bool succeeded)
    {
        if (code is null) return;
        lock (_lock)
        {
            if (!_tickets.TryGetValue(code, out var ticket)) return;
            ticket.Processing = false;
            if (succeeded) ticket.Completed = true;
        }
    }
}
