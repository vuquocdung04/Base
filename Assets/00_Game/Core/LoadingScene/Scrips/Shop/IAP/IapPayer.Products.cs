using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public static partial class IapPayer
{
    private static List<ProductDefinition> BuildDefinitions()
    {
        var list = new List<ProductDefinition>();

        PackCatalog catalog = _packs != null ? _packs.Catalog : null;
        if (catalog == null) return list;

        for (int i = 0; i < catalog.packs.Count; i++)
        {
            PackConfig pack = catalog.packs[i];
            if (pack == null || pack.cost.type != PackCostType.Iap) continue;
            if (string.IsNullOrEmpty(pack.cost.productId)) continue;

            list.Add(new ProductDefinition(
                pack.cost.productId,
                pack.IsOneTime ? ProductType.NonConsumable : ProductType.Consumable));
        }

        return list;
    }

    private static string GetPrice(PackConfig pack)
    {
        Product product = _controller != null ? _controller.GetProductById(pack.cost.productId) : null;
        string price = product != null && product.metadata != null ? product.metadata.localizedPriceString : null;

        return string.IsNullOrEmpty(price) ? pack.cost.fakePrice : price;
    }

    private static void OnProductsFetched(List<Product> products)
    {
        Debug.Log($"<color=cyan>[IAP]</color> Store tra ve {products.Count} product.");

        if (_packs != null) _packs.NotifyPricesChanged();
    }
}
