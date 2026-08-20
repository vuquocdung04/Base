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

        IapPayer.Bind(packs);

        UsingFakePayer = true;
        packs.BindPayer(PackCostType.Ads, PayFake);
        packs.BindPrice(PackCostType.Ads, pack => "ADS");
    }

    private static Awaitable<PurchaseTicket> PayFree(PackConfig pack) => Completed(PurchaseTicket.Ok());

    private static Awaitable<PurchaseTicket> PayCoin(PackConfig pack)
    {
        int price = Mathf.Max(0, pack.cost.amount);

        if (UseProfile.Coin < price)
        {
            ShowToast("Not enough Coin");
            return Completed(PurchaseTicket.Failed);
        }

        UseProfile.Coin -= price;
        Dispatcher.Instance.PostEvent(EventID.CHANGE_COIN);

        return Completed(PurchaseTicket.Ok());
    }

    private static Awaitable<PurchaseTicket> PayFake(PackConfig pack)
    {
        Debug.LogWarning($"[Pack] MUA GIA '{pack.packId}' ({pack.cost.type}) — chua noi store/ads that.");

        return Completed(PurchaseTicket.Ok());
    }

    private static void ShowToast(string message)
    {
        GameManager manager = GameManager.Instance;

        if (manager != null && manager.toastManager != null) manager.toastManager.ShowToast(message);
    }

    private static Awaitable<PurchaseTicket> Completed(PurchaseTicket ticket)
    {
        var source = new AwaitableCompletionSource<PurchaseTicket>();
        source.SetResult(ticket);

        return source.Awaitable;
    }
}
