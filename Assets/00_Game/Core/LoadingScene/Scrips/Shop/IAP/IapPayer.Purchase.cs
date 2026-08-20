using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public static partial class IapPayer
{
    private const float PurchaseTimeout = 180f;

    private static readonly Dictionary<string, AwaitableCompletionSource<PurchaseTicket>> _pending = new();

    private static Awaitable<PurchaseTicket> PayIap(PackConfig pack)
    {
        string productId = pack.cost.productId;

        if (_controller == null || string.IsNullOrEmpty(productId))
        {
            Debug.LogError($"[IAP] Store chua san sang hoac thieu productId cho '{pack.packId}'.");
            return Completed(PurchaseTicket.Failed);
        }

        Product product = _controller.GetProductById(productId);

        if (product == null)
        {
            Debug.LogError($"[IAP] Khong tim thay product '{productId}' tren store.");
            return Completed(PurchaseTicket.Failed);
        }

        var source = new AwaitableCompletionSource<PurchaseTicket>();
        _pending[productId] = source;

        Watchdog(productId, source).Forget();
        _controller.PurchaseProduct(product);

        return source.Awaitable;
    }

    private static void OnPending(PendingOrder order)
    {
        Resolve(order, PurchaseTicket.Ok(() => _controller.ConfirmPurchase(order)));
    }

    private static void OnConfirmed(Order order) => Resolve(order, PurchaseTicket.Ok());

    private static void OnFailed(FailedOrder order)
    {
        bool cancelled = order.FailureReason == PurchaseFailureReason.UserCancelled;

        if (cancelled) Debug.Log($"[IAP] Nguoi choi huy mua {ExtractProductId(order)}.");
        else Debug.LogWarning($"[IAP] Mua that bai: {order.FailureReason}");

        Resolve(order, PurchaseTicket.Failed);
    }

    private static void OnDeferred(DeferredOrder order)
    {
        Debug.LogWarning($"[IAP] Don '{ExtractProductId(order)}' dang cho duyet — chua giao hang.");

        Resolve(order, PurchaseTicket.Failed);
    }

    private static void Resolve(Order order, PurchaseTicket ticket)
    {
        string productId = ExtractProductId(order);
        if (string.IsNullOrEmpty(productId)) return;

        if (!_pending.TryGetValue(productId, out AwaitableCompletionSource<PurchaseTicket> source)) return;

        _pending.Remove(productId);
        source.SetResult(ticket);
    }

    private static async Awaitable Watchdog(string productId, AwaitableCompletionSource<PurchaseTicket> source)
    {
        await AwaitableEx.WaitRealtimeAsync(PurchaseTimeout);

        if (!_pending.TryGetValue(productId, out AwaitableCompletionSource<PurchaseTicket> current)) return;
        if (!ReferenceEquals(current, source)) return;

        _pending.Remove(productId);

        Debug.LogWarning($"[IAP] Qua {PurchaseTimeout}s khong co phan hoi cho '{productId}'.");
        source.SetResult(PurchaseTicket.Failed);
    }

    private static string ExtractProductId(Order order)
    {
        IReadOnlyList<CartItem> items = order != null && order.CartOrdered != null ? order.CartOrdered.Items() : null;
        if (items == null || items.Count == 0 || items[0] == null) return null;

        Product product = items[0].Product;
        return product != null && product.definition != null ? product.definition.id : null;
    }

    private static Awaitable<PurchaseTicket> Completed(PurchaseTicket ticket)
    {
        var source = new AwaitableCompletionSource<PurchaseTicket>();
        source.SetResult(ticket);

        return source.Awaitable;
    }
}
