using EventDispatcher;
using UnityEngine;

public partial class BoosterController : InitSingleton<BoosterController>
{
    public override void Init()
    {
        SeedData();
        ApplyConfigToItems();
        BindItems(true);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        BindItems(false);
    }

    private void BindItems(bool bind)
    {
        for (int i = 0; i < boosters.Count; i++)
        {
            BoosterItem item = boosters[i].item;
            if (item == null) continue;

            item.OnClickBuy -= HandleBuy;
            item.OnClickUse -= HandleUse;

            if (!bind) continue;

            item.OnClickBuy += HandleBuy;
            item.OnClickUse += HandleUse;
        }
    }

    private void HandleBuy(BoosterItem item) => PopupManager.Show<BuyBoosterBox>().Forget();

    private void HandleUse(BoosterItem item)
    {
        int index = IndexOf(item);
        if (index < 0) return;

        if (item.IsTutorial)
        {
            item.SetTutorial(false);
            GameScene.EnableDarkPanel(false);
        }

        SetTutorialDone(index);
        Consume(index);

        this.PostEvent(EventID.BOOSTER_USED, index);
    }

    public bool HasTutorial => FindTutorialIndex() >= 0;

    public async Awaitable ShowTutorialAsync()
    {
        int index = FindTutorialIndex();
        if (index < 0) return;

        BoosterItem item = GetItem(index);
        if (item == null) return;

        bool jumped = false;

        await PopupManager.Show<BoosterUnlockBox>(box => box.Setup(
            item.IconSprite,
            item.IconRect,
            () => GameScene.EnableWhitePanel(true),
            () =>
            {
                GameScene.EnableDarkPanel(true);
                GameScene.EnableWhitePanel(false);

                item.SetTutorial(true, tutorialSortingLayer, tutorialSortingOrder);
                jumped = true;
            }));

        await AwaitableEx.WaitUntil(() => jumped, destroyCancellationToken);
    }

    private int FindTutorialIndex()
    {
        for (int i = 0; i < boosters.Count; i++)
        {
            if (CurrentLevel == boosters[i].levelUnlock && !IsTutorialDone(i)) return i;
        }
        return -1;
    }
}
