using EventDispatcher;
using UnityEngine;

public partial class BoosterController : InitSingleton<BoosterController>
{
    public override void Init()
    {
        BuildLookup();
        SeedData();
        ApplyConfigToItems();
        BindItems(true);

        if (intro != null) intro.Init(catalog);

        BoosterData.OnAmountChanged += HandleAmountChanged;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        BindItems(false);

        BoosterData.OnAmountChanged -= HandleAmountChanged;
    }

    private void BindItems(bool bind)
    {
        for (int i = 0; i < items.Length; i++)
        {
            BoosterItem item = items[i];
            if (item == null) continue;

            item.OnClickBuy -= HandleBuy;
            item.OnClickUse -= HandleUse;

            if (!bind) continue;

            item.OnClickBuy += HandleBuy;
            item.OnClickUse += HandleUse;
        }
    }

    private void HandleAmountChanged(string id) => GetItem(id)?.SetQuantity(BoosterData.GetAmount(id));

    private void HandleBuy(BoosterItem item)
    {
        BoosterEntry entry = GetEntry(item.Id);

        PopupManager.Show<BuyBoosterBox>(box => box.SetContent(
            BoosterCatalogSO.IconOf(item.Id),
            entry != null ? entry.nameKey : string.Empty,
            entry != null ? entry.descKey : string.Empty,
            entry != null ? entry.price.ToString() : string.Empty)).Forget();
    }

    private void HandleUse(BoosterItem item)
    {
        string id = item.Id;
        if (string.IsNullOrEmpty(id)) return;

        if (item.IsTutorial)
        {
            item.SetTutorial(false);

            if (intro != null) intro.Hide();
        }

        BoosterData.SetTutorialDone(id);
        Consume(id);

        this.PostEvent(EventID.BOOSTER_USED, id);
    }

    public bool HasTutorial => FindTutorialItem() != null;

    public async Awaitable ShowTutorialAsync()
    {
        BoosterItem item = FindTutorialItem();
        if (item == null || intro == null) return;

        BoosterEntry entry = GetEntry(item.Id);
        if (entry == null) return;

        bool done = false;

        intro.Show(
            entry.levelUnlock,
            item.IconRect,
            () =>
            {
                GameScene.EnableWhitePanel(false);

                item.SetTutorial(true, tutorialSortingLayer, tutorialSortingOrder);
                done = true;
            },
            () => GameScene.EnableWhitePanel(true));

        await AwaitableEx.WaitUntil(() => done, destroyCancellationToken);
    }
}
