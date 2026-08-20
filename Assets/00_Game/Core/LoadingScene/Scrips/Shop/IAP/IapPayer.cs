using UnityEngine;
using UnityEngine.Purchasing;

public static partial class IapPayer
{
    private static StoreController _controller;
    private static PackManager _packs;

    public static bool IsReady => _controller != null;

    public static void Bind(PackManager packs)
    {
        if (packs == null) return;

        _packs = packs;

        packs.BindPayer(PackCostType.Iap, PayIap);
        packs.BindPrice(PackCostType.Iap, GetPrice);

        Connect().Forget();
    }

    private static async Awaitable Connect()
    {
        _controller = UnityIAPServices.StoreController();

        _controller.OnPurchasePending += OnPending;
        _controller.OnPurchaseConfirmed += OnConfirmed;
        _controller.OnPurchaseFailed += OnFailed;
        _controller.OnPurchaseDeferred += OnDeferred;

        _controller.OnProductsFetched += OnProductsFetched;
        _controller.OnPurchasesFetched += OnPurchasesFetched;

        await _controller.Connect();

        _controller.FetchProducts(BuildDefinitions());
        _controller.FetchPurchases();
    }
}
