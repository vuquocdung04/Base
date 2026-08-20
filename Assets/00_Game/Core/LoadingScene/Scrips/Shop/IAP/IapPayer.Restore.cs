using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public static partial class IapPayer
{
    public static void Restore()
    {
        if (_controller == null)
        {
            Debug.LogWarning("[IAP] Store chua san sang, chua Restore duoc.");
            return;
        }

        _controller.RestoreTransactions(OnRestoreFinished);
        _controller.FetchPurchases();
    }

    private static void OnRestoreFinished(bool success, string error)
    {
        if (success) Debug.Log("<color=cyan>[IAP]</color> Restore xong.");
        else Debug.LogWarning($"[IAP] Restore that bai: {error}");
    }

    private static void OnPurchasesFetched(Orders orders)
    {
        if (orders == null || _packs == null) return;

        RestoreOwned(orders.ConfirmedOrders);
    }

    private static void RestoreOwned(IReadOnlyList<ConfirmedOrder> confirmed)
    {
        if (confirmed == null) return;

        for (int i = 0; i < confirmed.Count; i++)
        {
            string productId = ExtractProductId(confirmed[i]);
            if (string.IsNullOrEmpty(productId)) continue;

            PackConfig pack = FindPackByProduct(productId);
            if (pack == null || !pack.IsOneTime) continue;

            _packs.RestorePurchase(pack);

            Debug.Log($"<color=cyan>[IAP]</color> Khoi phuc '{pack.packId}' tu store.");
        }
    }

    private static PackConfig FindPackByProduct(string productId)
    {
        PackCatalog catalog = _packs != null ? _packs.Catalog : null;
        if (catalog == null) return null;

        for (int i = 0; i < catalog.packs.Count; i++)
        {
            PackConfig pack = catalog.packs[i];
            if (pack != null && pack.cost.type == PackCostType.Iap && pack.cost.productId == productId) return pack;
        }

        return null;
    }
}
