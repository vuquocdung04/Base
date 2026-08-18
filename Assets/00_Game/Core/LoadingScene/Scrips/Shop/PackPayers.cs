using UnityEngine;
using Dispatcher = EventDispatcher.EventDispatcher;

public static class PackPayers
{
    public static bool UsingFakePayer { get; private set; }

    public static void Bind(PackManager packs)
    {
        if (packs == null) return;

        packs.BindPayer(PackCostType.Free, PayFree);
        packs.BindPrice(PackCostType.Free, pack => "FREE");

        packs.BindPayer(PackCostType.Coin, PayCoin);
        packs.BindPrice(PackCostType.Coin, pack => pack.cost.amount.ToString());

#if USE_IAP
        UsingFakePayer = false;
        IapPayer.Bind(packs);
#else
        UsingFakePayer = true;
        packs.BindPayer(PackCostType.Iap, PayFake);
        packs.BindPrice(PackCostType.Iap, pack => pack.cost.fakePrice);
#endif

        packs.BindPayer(PackCostType.Ads, PayFake);
        packs.BindPrice(PackCostType.Ads, pack => "ADS");
    }

    private static Awaitable<bool> PayFree(PackConfig pack) => Completed(true);

    private static Awaitable<bool> PayCoin(PackConfig pack)
    {
        int price = Mathf.Max(0, pack.cost.amount);

        if (UseProfile.Coin < price)
        {
            ShowToast("Not enough Coin");
            return Completed(false);
        }

        UseProfile.Coin -= price;
        Dispatcher.Instance.PostEvent(EventID.CHANGE_COIN);

        return Completed(true);
    }

    private static Awaitable<bool> PayFake(PackConfig pack)
    {
        Debug.LogWarning($"[Pack] MUA GIA '{pack.packId}' ({pack.cost.type}) — chua noi store/ads that.");

        return Completed(true);
    }

    private static void ShowToast(string message)
    {
        GameManager manager = GameManager.Instance;

        if (manager != null && manager.toastManager != null) manager.toastManager.ShowToast(message);
    }

    private static Awaitable<bool> Completed(bool value)
    {
        var source = new AwaitableCompletionSource<bool>();
        source.SetResult(value);

        return source.Awaitable;
    }
}
